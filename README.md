# GD2P01_Summative4
Unity game implementing basic AI in a Capture The Flag

#Instructions
WASD for movement
QE for swicthing between agents
ESC once game starts for menu

#Implementations
Agent behaviour is implemented using a central black board that they send and receive information from. The individual agent has behaviour for movement, for detecting hostiles, for evading hostiles, selecting to rescue agents or target flags, etc.. But it gets from the blackboard information about intruders, the go ahead to attack or pursue, updates on unsighted intruder positions, etc..
Attacking behaviour is a combination of GoTo target position and Flee from nearby hostiles. Default behaviour is to alternate between Wander and Idle.
Pursue is using GoTo to last known location. If the target is within range, then it would continue using GoTo to the target. GoTo was used as it was more reliable
Game ends once player captures all flags or gets all agents captured

#Notes
-movement is not disabled when in Esc Menu nor when game has ended