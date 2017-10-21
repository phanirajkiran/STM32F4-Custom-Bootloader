#include "stm32f4xx.h"                  // Device header
#include "HAL_GPIO_Driver.h"
#include "HAL_RCC_Driver.h"
#include "HAL_CRC_Driver.h"
#include "HAL_UART_Driver.h"

#define BOOT_FLAG_ADDRESS   0x08004000U

#define APPLICATION_START_ADDRESS   0x08008000U

static uint32_t CheckBootFlag(void);
static void JumpToApplication(void);
static void Bootloader_Init(void);

int main(void)
{
    
    UART_HandleTypeDef  uart_handle;
    GPIO_InitTypeDef    gpio_uart;
    
    gpio_uart.Pin = GPIO_PIN_2 | GPIO_PIN_3;
    gpio_uart.Mode = GPIO_MODE_AF_PP;
    gpio_uart.Pull = GPIO_PULL_NONE;
    gpio_uart.Speed = GPIO_SPEED_LOW;
    gpio_uart.Alternate = GPIO_AF7_USART2;
    
    HAL_RCC_GPIOA_CLK_ENABLE();
    HAL_GPIO_Init(GPIOA, &gpio_uart);
    
    uart_handle.Init.BaudRate = 115200;
    uart_handle.Init.Mode = HAL_UART_MODE_TX;
    uart_handle.Init.OverSampling = HAL_UART_OVERSAMPLING_16;
    uart_handle.Init.Parity = HAL_UART_PARITY_NONE;
    uart_handle.Init.StopBits = HAL_UART_STOP_1;
    uart_handle.Init.WordLength = HAL_UART_WORD8;
    uart_handle.Instance = USART2;
    
    HAL_RCC_USART2_CLK_ENABLE();
    HAL_UART_Init(&uart_handle);
    HAL_UART_Tx(&uart_handle, (uint8_t *)"Test\r\n", 6);
    if(CheckBootFlag() == 0)
    {
        JumpToApplication();
    }
    
    SystemCoreClockUpdate();
    
	for(;;)
	{
	}
	return 0;
}

/*! \brief Checks for Boot flag in memory.
 *  Checks in memory for the boot flag condition.
 *
 *  If a valid boot flag message is detected, then return 1. Else, return 0.
 *
 *  \retval uint8_t 1 = Valid boot flag detected. 0 = Invalid boot flag
 */
static uint32_t CheckBootFlag(void)
{
    return 0; 
}

/*! \brief Jumps to the main application.
 *
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
    
}
