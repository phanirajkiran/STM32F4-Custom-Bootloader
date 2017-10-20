#include "stm32f4xx.h"                  // Device header
#include "HAL_GPIO_Driver.h"
#include "HAL_RCC_Driver.h"
#include "HAL_CRC_Driver.h"

#define BOOT_FLAG_ADDRESS   (*(volatile uint32_t *)0x08004000U)
#define BOOT_FLAG           0x7F
int main(void)
{

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
static uint8_t Check_BootFlag(void)
{
    uint8_t *pboot_flag_data = (uint8_t *)BOOT_FLAG_ADDRESS;
    uint8_t boot_flag_data;
    uint32_t crc_check;
    
    /* Get the boot flag */
    boot_flag_data = *pboot_flag_data++;
    /* Determine if the boot flag is valid */
    if(boot_flag_data != BOOT_FLAG)
    {
        return 0;
    }

    /* Now ensure the data is valid */
    HAL_RCC_CRC_CLK_ENABLE();
    
    
}
