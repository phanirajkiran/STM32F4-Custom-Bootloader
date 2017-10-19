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
#define GPIO_PIN_0      ((uint16_t)0x0001)  /*!< Pin 0 selected    */
#define GPIO_PIN_1      ((uint16_t)0x0002)  /*!< Pin 1 selected    */
#define GPIO_PIN_2      ((uint16_t)0x0004)  /*!< Pin 2 selected    */
#define GPIO_PIN_3      ((uint16_t)0x0008)  /*!< Pin 3 selected    */
#define GPIO_PIN_4      ((uint16_t)0x0010)  /*!< Pin 4 selected    */
#define GPIO_PIN_5      ((uint16_t)0x0020)  /*!< Pin 5 selected    */
#define GPIO_PIN_6      ((uint16_t)0x0040)  /*!< Pin 6 selected    */
#define GPIO_PIN_7      ((uint16_t)0x0080)  /*!< Pin 7 selected    */
#define GPIO_PIN_8      ((uint16_t)0x0100)  /*!< Pin 8 selected    */
#define GPIO_PIN_9      ((uint16_t)0x0200)  /*!< Pin 9 selected    */
#define GPIO_PIN_10     ((uint16_t)0x0400)  /*!< Pin 10 selected   */
#define GPIO_PIN_11     ((uint16_t)0x0800)  /*!< Pin 11 selected   */
#define GPIO_PIN_12     ((uint16_t)0x1000)  /*!< Pin 12 selected   */
#define GPIO_PIN_13     ((uint16_t)0x2000)  /*!< Pin 13 selected   */
#define GPIO_PIN_14     ((uint16_t)0x4000)  /*!< Pin 14 selected   */
#define GPIO_PIN_15     ((uint16_t)0x8000)  /*!< Pin 15 selected   */
#define GPIO_PIN_ALL    ((uint16_t)0xFFFF)  /*!< All pins selected */
    
#define GPIO_MODE_INPUT                 0x00000000U /*!< Input Floating      */
#define GPIO_MODE_OUTPUT_PP             0x00000001U /*!< Output PushPull     */
#define GPIO_MODE_OUTPUT_OD             0x00000011U /*!< Output Open Drain   */
#define GPIO_MODE_AF_PP                 0x00000002U /*!< Alt. Fun. Push Pull */
#define GPIO_MODE_AF_OD                 0x00000012U /*!< Alt. Fun. Open Drain*/
#define GPIO_MODE_ANALOG                0x00000003U /*!< Analog              */
#define GPIO_MODE_IT_RISING             0x10110000U /*!< Ext. Interrupt with
rising edge trigger      */
#define GPIO_MODE_IT_FALLING            0x10210000U /*!< Ext. Interrupt with
falling edge trigger     */
#define GPIO_MODE_IT_RISING_FALLING     0x10310000U /*!< Ext. Interrupt with
rising & falling edge trigger  */
#define GPIO_MODE_EVT_RISING            0x10120000U /*!< Ext. event with
rising edge trigger detecting */ 
#define GPIO_MODE_EVT_FALLING           0x10220000U /*!< Ext. event with
falling edge trigger detecting */ 
#define GPIO_MODE_EVT_RISING_FALLING    0x10320000U /*!< Ext. event with
rising & falling edge trigger detecting */ 

#define GPIO_PULL_NONE  0x00000000U /*!< No Pull-up or Pull-down activation  */
#define GPIO_PULLUP     0x00000001U /*!< Pull-up activation                  */  
#define GPIO_PULLDOWN   0x00000002U /*!< Pull-down activation                */

#define GPIO_SPEED_LOW    0x00000000U /*!< Low speed, refer to datasheet     */
#define GPIO_SPEED_MED    0x00000001U /*!< Medium speed, refer to datasheet  */
#define GPIO_SPEED_FAST   0x00000002U /*!< Fast speed, refer to datasheet    */
#define GPIO_SPEED_HIGH   0x00000003U /*!< High speed, refer to datasheet    */

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