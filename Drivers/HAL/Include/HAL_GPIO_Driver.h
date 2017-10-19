/*! \file HAL_GPIO_Driver.h */
#ifndef _HAL_GPIO_DRIVER_H_
#define _HAL_GPIO_DRIVER_H_

#include "stm32f4xx.h"                  // Device header
#include "HAL_Common.h"

#ifdef __cplusplus
extern "C" {
#endif
/*****************************************************************************/
/*****************************************************************************/
/*****************************************************************************/
#define GPIO_PIN_0      (1U << 0)
#define GPIO_PIN_1      (1U << 1)
#define GPIO_PIN_2      (1U << 2)
#define GPIO_PIN_3      (1U << 3)
#define GPIO_PIN_4      (1U << 4)
#define GPIO_PIN_5      (1U << 5)
#define GPIO_PIN_6      (1U << 6)
#define GPIO_PIN_7      (1U << 7)
#define GPIO_PIN_8      (1U << 8)
#define GPIO_PIN_9      (1U << 9)
#define GPIO_PIN_10     (1U << 10)
#define GPIO_PIN_11     (1U << 11)
#define GPIO_PIN_12     (1U << 12)
#define GPIO_PIN_13     (1U << 13)
#define GPIO_PIN_14     (1U << 14)
#define GPIO_PIN_15     (1U << 15)
#define GPIO_PIN_ALL    (0xFF)
#define GPIO_MASK
    
#define GPIO_MODE_INPUT                 
#define GPIO_MODE_OUTPUT_PP             
#define GPIO_MODE_OUTPUT_OD             
#define GPIO_MODE_AF_PP                 
#define GPIO_MODE_AF_OD                 
#define GPIO_MODE_ANALOG                
#define GPIO_MODE_IT_RISING             
#define GPIO_MODE_IT_FALLING            
#define GPIO_MODE_IT_RISING_FALLING     
#define GPIO_MODE_EVT_RISING            
#define GPIO_MODE_EVT_FALLING           
#define GPIO_MODE_EVT_RISING_FALLING    

#define GPIO_PULL_NONE  (0x00)
#define GPIO_PULLUP     (0x01)    
#define GPIO_PULLDOWN   (0x10)

#define GPIO_SPEED_LOW
#define GPIO_SPEED_HIGH
#define GPIO_SPEED_FAST

/*****************************************************************************/
/*****************************************************************************/
/*****************************************************************************/

/*! \brief GPIO Initialization Structure
 *      
 *  Structure used to initialize the GPIO
 */
typedef struct GPIO_InitTypeDef
{
    uint32_t Pin;        /*!< The pin to initialize                          */
    uint32_t Mode;       /*!< The mode to set the gpio pin to                */
    uint32_t Pull;       /*!< Whether the pin is pull-up/down or no pull     */
    uint32_t Speed;      /*!< Speed of the pin                               */
    uint32_t Alternate;  /*!< Specifies if the pin is in alternate function  */
};

/*****************************************************************************/
/*                       Driver Exposed HAL                                  */
/*****************************************************************************/

/*! \brief Initializes the given GPIO 
 *  Initializes the given GPIO using the GPIO_Init handle.
 *  
 *  \param  *GPIOx      The GPIO base port address.
 *  \param  gpio_handle The GPIO initialization structure handle
 *  \retval uint8_t     Returns 1 if successful. 0 if failed.
 *    
 */
uint8_t HAL_GPIO_Init(GPIO_TypeDef *GPIOx, GPIO_InitTypeDef gpio_handle);

/*! \brief Reads from the specified GPIO pin
 *  
 *  \param  *GPIOx      The GPIO base port address.
 *  \param  gpio_pin    The GPIO pin number to read from
 *  \retval uint8_t     The value of the GPIO pin
 */
uint8_t HAL_GPIO_ReadPin(GPIO_TypeDef *GPIOx, uint16_t gpio_pin);

/*! \brief Writes to the specified GPIO pin
 *  
 *  \param  *GPIOx      The GPIO base port address.
 *  \param  gpio_pin    The GPIO pin number to read from
 *  \retval uint8_t     The value of the GPIO pin
 */
void HAL_GPIO_WritePin(GPIO_TypeDef *GPIOx, uint16_t gpio_pin, uint8_t val);

/*! \brief Writes to the specified GPIO pin
 *  
 *  \param  *GPIOx      The GPIO base port address.
 *  \param  gpio_pin    The GPIO pin number to read from
 */
void HAL_GPIO_TogglePin(GPIO_TypeDef *GPIOx, uint16_t gpio_pin);

#ifdef __cplusplus
}
#endif
#endif /* _HAL_GPIO_DRIVER_H_ */