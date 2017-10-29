Current Status: In Progress

# STM32F4 Custom Bootloader
This is an example of a custom bootloader for the STM32F4 series. More specifically, this project uses the STM32F411RE ARM-Cortex M4 microcontroller. 

The bootloader resides in Sector 0 of the main memory block in flash (0x0800 0000 - 0x0800 3FFF), while the main application should reside in sector 2 (starting address 0x0800 8000).

A flash utility made in C# WPF is used to download the raw binary file of the main application from the host to the target.

Folder Reg_Blinky is a blinky program to work with the bootloader. Use the Reg_Blinky.bin to flash the program via the flash utility program. 

Image of the program:

![](Bootloader_Flash_Util.png =300x410)