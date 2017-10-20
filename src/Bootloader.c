#include "stm32f4xx.h"                  // Device header
#include "HAL_GPIO_Driver.h"
#include "HAL_RCC_Driver.h"
#include "HAL_CRC_Driver.h"

#define BOOT_FLAG_ADDRESS   (*(volatile uint32_t *)0x08004000U)
#define BOOT_FLAG           0x7F
#define BOOT_FLAG_SIZE      5U

int main(void)
{
    volatile uint32_t *pAddress = (__IO uint32_t*) 0x08004000U;
    uint32_t crc;
    /* Writing a dummy boot flag to the sector */
    /* Starting at Sector 1 (0x08004000) */
    /* 1: Unlock Flash */
    while((FLASH->SR & FLASH_SR_BSY));
    FLASH->KEYR = 0x45670123U;
    FLASH->KEYR = 0xCDEF89ABU;
    /* 2: Erase sector 1 */
    while((FLASH->SR & FLASH_SR_BSY));
    FLASH->CR |= FLASH_CR_PSIZE_1;
    FLASH->CR |= FLASH_CR_SER;  // Set the Sector Erase Bit
    FLASH->CR |= (0x01 << FLASH_CR_SNB_Pos);  // Set to Sector 1
    FLASH->CR |= FLASH_CR_STRT; // Start flash erase operation
    while((FLASH->SR & FLASH_SR_BSY)); //Wait until BUSY bit is cleared
    /* 3: Program a value into the flash: 0xAA */
    while((FLASH->SR & FLASH_SR_BSY)); //Wait until BUSY bit is cleared
    FLASH->CR |= FLASH_CR_PG; // Enable flash programming
    *pAddress = 0xAABBCCDD;
    pAddress += 1;
    *pAddress = 0xwhy;
    
    
    
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
static uint32_t Check_BootFlag(void)
{
    /* Get the boot flag */
    uint8_t boot_flag_data = *(uint8_t *)BOOT_FLAG_ADDRESS; 
    uint32_t *pboot_flag_data = (uint32_t *)BOOT_FLAG_ADDRESS;
    uint8_t boot_flag_size = BOOT_FLAG_SIZE;
    uint32_t crc_result;
    

    /* Determine if the boot flag is valid */
    if(boot_flag_data != BOOT_FLAG)
    {
        return 0;
    }

    /* Now ensure the data is valid */
    HAL_RCC_CRC_CLK_ENABLE();
    while(boot_flag_size--)
        CRC->DR = *pboot_flag_data++;
    
    crc_result = CRC->DR;
    
    HAL_RCC_CRC_CLK_DISABLE();
    
    if(crc_result == 0)
        return 1;
    else
        return 0;
    
    
}
