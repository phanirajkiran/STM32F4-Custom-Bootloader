#findef _HAL_FLASH_DRIVER_H_
#define _HAL_FLASH_DRIVER_H_

#include "stm32f4xx.h"                  // Device header
#include "HAL_Common.h"

#ifdef __cplusplus
extern "C" {
#endif
/*****************************************************************************/
/*****************************************************************************/
/*****************************************************************************/
#define HAL_FLASH_TYPEERASE_MASS    FLASH_CR_MER /*!< Mass erase             */
#define HAL_FLASH_TYPEERASE_SECTOR  FLASH_CR_SER /*!< Sector erase           */
        
#define HAL_FLASH_SECTOR_0          0x0
#define HAL_FLASH_SECTOR_1          0x1
#define HAL_FLASH_SECTOR_2          0x2
#define HAL_FLASH_SECTOR_3          0x3
#define HAL_FLASH_SECTOR_4          0x4
#define HAL_FLASH_SECTOR_5          0x5
#define HAL_FLASH_SECTOR_6          0x6
#define HAL_FLASH_SECTOR_7          0x7

#define FLASH_KEYR_1                0x45670123U
#define FLASH_KEYR_2                0xCDEF89ABU
/*****************************************************************************/
/*****************************************************************************/
/*****************************************************************************/
/*! \brief  Configuration struct for erasing the flash
 */
typedef struct 
{
    uint32_t TypeErase; /*!< Mass erase or sector erase                      */
    uint32_t Sector;    /*!< Initial sector to erase when not mass erasing   */
    uint32_t NbSectors; /*!< Number of sectors to erase. Between 1 and
                             (max number of sectors - value of initial sector */
}Flash_EraseInitTypeDef;
    
    
/*****************************************************************************/
/*                       Driver Exposed HAL                                  */
/*****************************************************************************/
/*! \brief  Unlocks the FLASH control register access
 */
void HAL_Flash_Unlock(void);
    
/*! \brief  Locks the FLASH control register access
 */
void HAL_Flash_Lock(void);
    
/*! \brief Perform a mass erase or erase the specified FLASh memory sectors.
 *  
 *  \param  *pEraseInit     pointer to a erase configuration structure
 *  \param  *SectorError    pointer to variable that contains the config on
 *                          faulty sector in case of error.
 *                          0xFFFFFFFFU means that all the sectors have been
 *                          correctly erased
 */
void HAL_Flash_Erase(Flash_EraseInitTypedef *pEraseinit, uint32_t *SectorError);
    

#ifdef __cplusplus
}
#endif

#endif
