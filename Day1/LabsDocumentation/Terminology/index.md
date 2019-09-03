# Terminology

* **ADC**: [ADC](https://en.wikipedia.org/wiki/Analog-to-digital_converter) stands for *Analog to Digital Converter*. Just as its name indicates, an ADC converts an analog signal into a digital signal. In the case of this lab, it converts the analog voltage to a digital number representing the magnitude of the said voltage. The ADC in the lab design is the MCP3008 chip. This chip reads an analog signal from the Water Level sensor, and converts it to a digital signal, communicating it back to the Pi through SPI. The Raspberry Pi is not equipped with an ADC, so we must include one in our design to properly retrieve the data from the water level sensor.

* **GPIO**: [GPIO](https://en.wikipedia.org/wiki/General-purpose_input/output) stands for *General Purpose Input Output*, can be used for a number of purposes, including turning an LED on (HIGH) or off (LOW). In this lab, numerous GPIO pins are used to power and read sensor data.

* **SPI**: [SPI](https://en.wikipedia.org/wiki/Serial_Peripheral_Interface) stands for *Serial Peripheral Interface*. On the Raspberry Pi there are two SPI *busses* (SPI0 and SPI1) each made up of 4 GPIO pins (aka Signals). SPI works in a master-slave fashion, meaning the master which is in our case the Pi, will essentially be obtaining signals from the slave (the ADC). The four signals are:
    * SCLK - Serial Clock
    * MOSI - Master Output, Slave Input
    * MISO - Master Input, Slave Output
    * CS - Chip/Slave Select (since we are using only one slave, this will be set to LOW)
As you can see from the signal definitions, SPI communicates in full duplex between the Master and the Slave.