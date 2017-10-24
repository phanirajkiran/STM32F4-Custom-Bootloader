Current Status: In Progress

# STM32F4 Custom Bootloader
This is an example of a custom bootloader for the STM32F4 series. More specifically, this project uses the STM32F411RE ARM-Cortex M4 microcontroller. 

The bootloader resides in Sector 0 of the main memory block in flash (0x0800 0000 - 0x0800 3FFF), while the main application should reside in sector 2 (0x0800 8000 - 0x0800 FFFF).

A flash utility made in C# WPF is used to download the raw binary file of the main application from the host to the target.
