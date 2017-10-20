/*! \file HAL_RCC_Driver.h */
#ifndef _HAL_RCC_DRIVER_H_
#define _HAL_RCC_DRIVER_H_

#include "stm32f4xx.h"                  // Device header
#include "HAL_Common.h"

#ifdef __cplusplus
extern "C" {
#endif
/*****************************************************************************/
/*****************************************************************************/
/*****************************************************************************/
#define HAL_RCC_GPIOA_CLK_ENABLE()      RCC->AHB1ENR |= RCC_AHB1ENR_GPIOAEN;
#define HAL_RCC_GPIOA_CLK_DISABLE()     RCC->AHB1ENR &= ~RCC_AHB1ENR_GPIOAEN;
#define HAL_RCC_GPIOB_CLK_ENABLE()      RCC->AHB1ENR |= RCC_AHB1ENR_GPIOBEN;
#define HAL_RCC_GPIOB_CLK_DISABLE()     RCC->AHB1ENR &= ~RCC_AHB1ENR_GPIOBEN;
#define HAL_RCC_GPIOC_CLK_ENABLE()      RCC->AHB1ENR |= RCC_AHB1ENR_GPIOCEN;
#define HAL_RCC_GPIOC_CLK_DISABLE()     RCC->AHB1ENR &= ~RCC_AHB1ENR_GPIOCEN;
#define HAL_RCC_GPIOD_CLK_ENABLE()      RCC->AHB1ENR |= RCC_AHB1ENR_GPIODEN;
#define HAL_RCC_GPIOD_CLK_DISABLE()     RCC->AHB1ENR &= ~RCC_AHB1ENR_GPIODEN;
#define HAL_RCC_GPIOE_CLK_ENABLE()      RCC->AHB1ENR |= RCC_AHB1ENR_GPIOEEN;
#define HAL_RCC_GPIOE_CLK_DISABLE()     RCC->AHB1ENR &= ~RCC_AHB1ENR_GPIOEEN;
#define HAL_RCC_GPIOH_CLK_ENABLE()      RCC->AHB1ENR |= RCC_AHB1ENR_GPIOHEN;
#define HAL_RCC_GPIOH_CLK_DISABLE()     RCC->AHB1ENR &= ~RCC_AHB1ENR_GPIOHEN;
#define HAL_RCC_CRC_CLK_ENABLE()        RCC->AHB1ENR |= RCC_AHB1ENR_CRCEN;
#define HAL_RCC_CRC_CLK_DISABLE()       RCC->AHB1ENR &= ~RCC_AHB1ENR_CRCEN;
#define HAL_RCC_DMA1_CLK_ENABLE()       RCC->AHB1ENR |= RCC_AHB1ENR_DMA1EN;
#define HAL_RCC_DMA1_CLK_DISABLE()      RCC->AHB1ENR &= ~RCC_AHB1ENR_DMA1EN;
#define HAL_RCC_DMA2_CLK_ENABLE()       RCC->AHB1ENR |= RCC_AHB1ENR_DMA2EN;
#define HAL_RCC_DMA2_CLK_DISABLE()      RCC->AHB1ENR &= ~RCC_AHB1ENR_DMA2EN;

/*****************************************************************************/
/*****************************************************************************/
/*****************************************************************************/


/*****************************************************************************/
/*                       Driver Exposed HAL                                  */
/*****************************************************************************/


#ifdef __cplusplus
}
#endif
#endif /* _HAL_RCC_DRIVER_H_ */
