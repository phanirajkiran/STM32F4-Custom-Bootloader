/*! \file HAL_UART_Driver.h */
#ifndef _HAL_UART_DRIVER_H_
#define _HAL_UART_DRIVER_H_

#include "stm32f4xx.h"                  // Device header
#include "HAL_Common.h"

#ifdef __cplusplus
extern "C" {
#endif
/*****************************************************************************/
/*****************************************************************************/
/*****************************************************************************/
#define HAL_UART_ENABLE()
#define HAL_UART_DISABLE()

/*****************************************************************************/
/*****************************************************************************/
/*****************************************************************************/
typedef struct
{
    uint32_t BaudRate;
    uint32_t WordLength;
    uint32_t StopBits;
    uint32_t Parity;
    uint32_t Mode;
    uint32_t HwFlowCtl;
    uint32_t OverSampling;
} UART_InitTypeDef;

typedef struct
{
    USART_TypeDef *Instance;
    UART_InitTypeDef Init;
    uint8_t *pTxBuffPtr;
    uint16_t TxXferSize;
    __IO uint16_t TxXferCount;
    uint8_t *pRxBuffPtr;
    uint16_t RxXferSize;
    __IO uint16_t RxXferSize;
    uint32_t TxState;
    uint32_t RxState;
    uint32_t ErrorCode;
}UART_HandleTypeDef;
/*****************************************************************************/
/*                       Driver Exposed HAL                                  */
/*****************************************************************************/


#ifdef __cplusplus
}
#endif
#endif /* _HAL_UART_DRIVER_H_ */
