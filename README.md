# Nestacular

Early alpha



| Milestone            | Progress |
| -------------        | ------------- |
| CPU Emulation        | 75% |
| PPU Emulation        | 2%  |
| APU Emulation        | No  |
| Controller Emulation | No  |
| Mapper Support       | 1%  |
| Save States          | No  |
| Rewind               | No  |
| Fast Forward         | No  |
| Instruction Display  | 50% |
| Pallette Display     | 0%  |

#### Mappers Supported 
NES Kind of...  

## Performance
| Component | Time |
| --------- | ---- |
| CPU       | <1MS Per 30k Instructions |
| PPU/Bitmap Rendering | 450MS Per 1 frame |
| APU       | N/A  |
| Input Latency | N/A |
| Ram       | ~75MB |
| CPU       | UNK. |

## Some Notes and Considerations
Cycle times are not accurately represented with most instructions consuming a default of 7 cycles per.  
Timing is controlled currently by FPS, each frame of the engine that is rendered, the Emulator tries to compute and produce one NES frame.  
A NES frame time is controlled by the PPU, a single NES frame takes exactly 89342 PPU cycles to perform, with a CPU cycle happening every 3 PPU cycles.  
As long that the emulator can perform all 89342 PPU cycles before the next frame render by the engine, then it will run at 'full speed' (60fps) otherwise lag frames will be generated (where no on screen updates occur and inputs will be buffered.  

Each PPU cycle generates a pixel, with x,y, and color data, and additional appropriate metadata if needed.  
I would like this emulator to be a cool excersize in showing what the NES is doing, therefore the goal is cycle accurate, with lots of debug information, as well as the ability to step forward and backward in execution all the way down to the clock cycle.  
This means I do not expect it to be the fasted emulator, or even playable.  
I expect hardware requirements to increase, ram usage to be high, multi core CPU to be necassary. Potentially even requiring bare metal, RTOS, FPGA or something else in the future.  

The Emulator can be paused by pressing space, and the execution mode can be changed by pressing down, stepping through frames/instructions, can be done with the right arrow depending on execution.

Overall though, this is just fun for me
