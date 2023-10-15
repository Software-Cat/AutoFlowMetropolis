![Untitled](Logo.png)

# AutoFlow Metropolis

---

## Authors

Naman Doshi, James Jiang, Xuanyu Liu, Bowen Wu

---

## Overview

AutoFlow Metropolis is a large scale traffic simulation framework designed to evaluate AutoFlow algorithm performance.

Also check out our backend, AutoFlow Engine! https://github.com/naman-doshi/AutoFlow

---

## Running

1. Install Unity Hub
2. Install Unity `2022.1` or later (earlier versions may work but there may be compatibility issues)
3. Clone the project `git clone https://github.com/Software-Cat/AutoFlowMetropolis.git`
4. Add the cloned folder as a project to Unity Hub
5. Open the project
6. Clone AutoFlow's backend server
7. In the root directory of AutoFlow backend, run `./server.cmd`. Choose "Yes" to use AutoFlow and "No" to use the selfish algorithm for comparison.
8. Start Unity's play mode
9. Enjoy!

**How can I see the Unity project if I don't have Unity installed?**

1. You can check out the scripts implemented in C# at [/Assets/WebSocketTraffic](https://github.com/Software-Cat/AutoFlowMetropolis/tree/main/Assets/WebSocketTraffic). They are highly readable even without Unity experience.
2. You can check out some photos of the Unity project in our design document, or our video.
