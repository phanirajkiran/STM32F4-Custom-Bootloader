#include "stm32f4xx.h"                  // Device header
#include "HAL_GPIO_Driver.h"
#include "HAL_RCC_Driver.h"
#include "HAL_CRC_Driver.h"
#include "HAL_UART_Driver.h"
#include "HAL_Flash_Driver.h"

#define BOOT_FLAG_ADDRESS           0x08004000U
#define APPLICATION_START_ADDRESS   0x08008000U
#define TIMEOUT_VALUE               SystemCoreClock/4

#define ACK     0x06U
#define NACK    0x16U

/*****************************************************************************/
/*                          Private Variables                                */
/*****************************************************************************/
/*! \brief The uart handle
 */
static UART_HandleTypeDef UartHandle;

/*! \brief Buffer for received messages
 */
static uint8_t pRxBuffer[32];

typedef enum
{
    ERASE = 0x43,
    WRITE = 0x31,
    CHECK = 0x51,
    JUMP  = 0xA1,
} COMMANDS;

/*****************************************************************************/
/*                     Private Function Prototypes                           */
/*****************************************************************************/
/*! \brief Jumps to the main application.
 */
static void JumpToApplication(void);

/*! \brief  Initializes the bootloader for host communication.
 *          Communication will be done through the UART peripheral.
 */
static void Bootloader_Init(void);

/*! \brief Sends an ACKnowledge byte to the host.
 *  
 *  \param  *UartHandle The UART handle
 */
static void Send_ACK(UART_HandleTypeDef *UartHandle);

/*! \brief Sends an NACKnowledge byte to the host.
 *  
 *  \param  *UartHandle The UART handle
 */
static void Send_NACK(UART_HandleTypeDef *UartHandle);

/*! \brief Validates the checksum of the message.
 *  Validates the received message through a simple checksum.
 *
 *  Each byte of the message (including the received checksum) is XOR'd with 
 *  the previously calculated checksum value. The result should be 0x00.
 *  Note: The first byte of the message is XOR'd with an initial value of 0xFF
 *  
 *  \param  *pBuffer    The buffer where the message is stored.
 *  \param  len         The length of the message;
 *  \retval uint8_t     The result of the validation. 1 = OK. 0 = FAIL
 */
static uint8_t CheckChecksum(uint8_t *pBuffer, uint32_t len);

/*! \brief Erase flash function
 */
static void Erase(void);

/*! \brief Write flash function
 */
static void Write(void);

/*! \brief Check flashed image
 */
static void Check(void);

int main(void)
{
    SystemCoreClockUpdate();
    Bootloader_Init();
    
    /* Hookup Host and Target                           */
    /* First send an ACK. Host should reply with ACK    */
    /* If no valid ACK is received within TIMEOUT_VALUE */
    /* then jump to main application                    */
    Send_ACK(&UartHandle);
    if(HAL_UART_Rx(&UartHandle, pRxBuffer, 2, TIMEOUT_VALUE) == HAL_UART_TIMEOUT)
    {
        Send_NACK(&UartHandle);
        JumpToApplication();
    }
    if(CheckChecksum(pRxBuffer, 2) != 1 || pRxBuffer[0] != ACK)
    {
        Send_NACK(&UartHandle);
        JumpToApplication();
    }
    
    /* At this point, hookup communication is complete */
    /* Wait for commands and execute accordingly       */
    
	for(;;)
	{
        // wait for a command
        while(HAL_UART_Rx(&UartHandle, pRxBuffer, 2, TIMEOUT_VALUE) == HAL_UART_TIMEOUT);
        
        if(CheckChecksum(pRxBuffer, 2) != 1)
        {
            Send_NACK(&UartHandle);
        }
        else
        {
            switch(pRxBuffer[0])
            {
                case ERASE:
                    Send_ACK(&UartHandle);
                    Erase();
                    break;
                case WRITE:
                    Send_ACK(&UartHandle);
                    Write();
                    break;
                case CHECK:
                    Send_ACK(&UartHandle);
                    Check();
                    break;
                case JUMP:
                    Send_ACK(&UartHandle);
                    JumpToApplication();
                    break;
                default: // Unsupported command
                    Send_NACK(&UartHandle);
                    break;
            }
        }
        
	}
    
    for(;;);
    
	return 0;
}

/*! \brief Jumps to the main application.
 */
static void JumpToApplication(void)
{
    if (((*(__IO uint32_t*)APPLICATION_START_ADDRESS) & 0x2FFE0000 ) == 0x20000000)
    {
        /* First, disable all IRQs */
        __disable_irq();

        /* Get the main application start address */
        uint32_t jump_address = *(__IO uint32_t *)(APPLICATION_START_ADDRESS + 4);

        /* Set the main stack pointer to to the application start address */
        __set_MSP(*(__IO uint32_t *)APPLICATION_START_ADDRESS);

        // Create function pointer for the main application
        void (*pmain_app)(void) = (void (*)(void))(jump_address);

        // Now jump to the main application
        pmain_app();
    }
    
}

/*! \brief  Initializes the bootloader for host communication.
 *          Communication will be done through the UART peripheral.
 */
static void Bootloader_Init(void)
{
    GPIO_InitTypeDef gpio_uart;
    
    gpio_uart.Pin = GPIO_PIN_2 | GPIO_PIN_3;
    gpio_uart.Mode = GPIO_MODE_AF_PP;
    gpio_uart.Pull = GPIO_PULL_NONE;
    gpio_uart.Speed = GPIO_SPEED_LOW;
    gpio_uart.Alternate = GPIO_AF7_USART2;
    
    HAL_RCC_GPIOA_CLK_ENABLE();
    HAL_GPIO_Init(GPIOA, &gpio_uart);
    
    UartHandle.Init.BaudRate = 115200;
    UartHandle.Init.Mode = HAL_UART_MODE_TX_RX;
    UartHandle.Init.OverSampling = HAL_UART_OVERSAMPLING_16;
    UartHandle.Init.Parity = HAL_UART_PARITY_NONE;
    UartHandle.Init.StopBits = HAL_UART_STOP_1;
    UartHandle.Init.WordLength = HAL_UART_WORD8;
    UartHandle.Instance = USART2;
    
    HAL_RCC_USART2_CLK_ENABLE();
    HAL_UART_Init(&UartHandle);
}

/*! \brief Sends an ACKnowledge byte to the host.
 *  
 *  \param  *UartHandle The UART handle
 */
static void Send_ACK(UART_HandleTypeDef *handle)
{
    uint8_t msg[2] = {ACK, ACK};
    
    HAL_UART_Tx(handle, msg, 2);
}

/*! \brief Sends an NACKnowledge byte to the host.
 *  
 *  \param  *UartHandle The UART handle
 */
static void Send_NACK(UART_HandleTypeDef *handle)
{
    uint8_t msg[2] = {NACK, NACK};
    
    HAL_UART_Tx(handle, msg, 2);
}

/*! \brief Validates the checksum of the message.
 *  Validates the received message through a simple checksum.
 *
 *  Each byte of the message (including the received checksum) is XOR'd with 
 *  the previously calculated checksum value. The result should be 0x00.
 *  Note: The first byte of the message is XOR'd with an initial value of 0xFF
 *  
 *  \param  *pBuffer    The buffer where the message is stored.
 *  \param  len         The length of the message;
 *  \retval uint8_t     The result of the validation. 1 = OK. 0 = FAIL
 */
static uint8_t CheckChecksum(uint8_t *pBuffer, uint32_t len)
{
    uint8_t initial = 0xFF;
    uint8_t result = 0x7F; /* some random result value */
    
    result = initial ^ *pBuffer++;
    len--;
    while(len--)
    {
        result ^= *pBuffer++;
    }
    
    result ^= 0xFF;
    
    if(result == 0x00)
    {
        return 1;
    }
    else
    {
        return 0;
    }
}

/*! \brief Erase flash function
 */
static void Erase(void)
{
    Flash_EraseInitTypeDef flashEraseConfig;
    uint32_t sectorError;
    
    // Receive the number of pages to be erased (1 byte)
    // the initial sector to erase  (1 byte)
    // and the checksum             (1 byte)
    while(HAL_UART_Rx(&UartHandle, pRxBuffer, 3, TIMEOUT_VALUE) == HAL_UART_TIMEOUT);
    // validate checksum
    if(CheckChecksum(pRxBuffer, 3) != 1)
    {
        Send_NACK(&UartHandle);
        return;
    }
    
    if(pRxBuffer[0] == 0xFF)
    {
        // global erase: not supported
        Send_NACK(&UartHandle);
    }
    else
    {
        // Sector erase:
        flashEraseConfig.TypeErase = HAL_FLASH_TYPEERASE_SECTOR;
        
        // Set the number of sectors to erase
        flashEraseConfig.NbSectors = pRxBuffer[0];
        
        // Set the initial sector to erase
        flashEraseConfig.Sector = pRxBuffer[1];
        
        // perform erase
        HAL_Flash_Unlock();
        HAL_Flash_Erase(&flashEraseConfig, &sectorError);
        HAL_Flash_Lock();
        
        Send_ACK(&UartHandle);
    }
}


/*! \brief Write flash function
 */
static void Write(void)
{
    uint8_t numBytes;
    uint32_t startingAddress = 0;
    uint8_t i;
    // Receive the starting address and checksum
    // Address = 4 bytes
    // Checksum = 1 byte
    while(HAL_UART_Rx(&UartHandle, pRxBuffer, 5, TIMEOUT_VALUE) == HAL_UART_TIMEOUT);
    
    // Check checksum
    if(CheckChecksum(pRxBuffer, 5) != 1)
    {
        // invalid checksum
        Send_NACK(&UartHandle);
        return;
    }
    else
    {
        Send_ACK(&UartHandle);
    }
    
    // Set the starting address
    startingAddress = pRxBuffer[0] + (pRxBuffer[1] << 8) 
                    + (pRxBuffer[2] << 16) + (pRxBuffer[3] << 24);
    
    // Receive the number of bytes to be written
    while(HAL_UART_Rx(&UartHandle, pRxBuffer, 2, TIMEOUT_VALUE) == HAL_UART_TIMEOUT);
    numBytes = pRxBuffer[0];
    
    // Receive the data
    while(HAL_UART_Rx(&UartHandle, pRxBuffer, numBytes+1, TIMEOUT_VALUE) == HAL_UART_TIMEOUT);
    
    // Check checksum of received data
    if(CheckChecksum(pRxBuffer, 5) != 1)
    {
        // invalid checksum
        Send_NACK(&UartHandle);
        return;
    }
    
    // valid checksum at this point
    // Program flash with the data
    i = 0;
    HAL_Flash_Unlock();
    while(numBytes--)
    {
        HAL_Flash_Program(FLASH_TYPEPROGRAM_BYTE, startingAddress, pRxBuffer[i]);
        startingAddress++;
        i++; 
    }
    HAL_Flash_Lock();
    
    // Send ACK
    Send_ACK(&UartHandle);
}

/*! \brief Check flashed image
 */
static void Check(void)
{
    uint32_t startingAddress = 0;
    uint32_t endingAddress = 0;
    uint32_t address;
    uint32_t *data;
    uint32_t crcResult;
    
    // Receive the starting address and checksum
    // Address = 4 bytes
    // Checksum = 1 byte
    while(HAL_UART_Rx(&UartHandle, pRxBuffer, 5, TIMEOUT_VALUE) == HAL_UART_TIMEOUT);
    
    // Check checksum
    if(CheckChecksum(pRxBuffer, 5) != 1)
    {
        // invalid checksum
        Send_NACK(&UartHandle);
        return;
    }
    else
    {
        Send_ACK(&UartHandle);
    }
    
    // Set the starting address
    startingAddress = pRxBuffer[0] + (pRxBuffer[1] << 8) 
                    + (pRxBuffer[2] << 16) + (pRxBuffer[3] << 24);
    
    // Receive the ending address and checksum
    // Address = 4 bytes
    // Checksum = 1 byte
    while(HAL_UART_Rx(&UartHandle, pRxBuffer, 5, TIMEOUT_VALUE) == HAL_UART_TIMEOUT);
    
    // Check checksum
    if(CheckChecksum(pRxBuffer, 5) != 1)
    {
        // invalid checksum
        Send_NACK(&UartHandle);
        return;
    }
    else
    {
        Send_ACK(&UartHandle);
    }
    
    // Set the starting address
    endingAddress = pRxBuffer[0] + (pRxBuffer[1] << 8) 
                    + (pRxBuffer[2] << 16) + (pRxBuffer[3] << 24);
    
    HAL_RCC_CRC_CLK_ENABLE();
    data = (uint32_t *)((__IO uint32_t*) startingAddress);
    for(address = startingAddress; address < endingAddress; address += 4)
    {
        data = (uint32_t *)((__IO uint32_t*) address);
        crcResult = HAL_CRC_Accumulate(data, 1);
    }
    
    HAL_RCC_CRC_CLK_DISABLE();
    if(crcResult == 0x00)
    {
        Send_ACK(&UartHandle);
    }
    else
    {
        Send_NACK(&UartHandle);
    }
    
    JumpToApplication();
}
