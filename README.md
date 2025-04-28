# XCOM Style field of view/ line of sight system for Unity 

This script implements a field of view detection system inspired by modern XCOM games, where:

1. A sphere cast identifies potential targets within a specified radius

2. A primary visibility check occurs along a direct line from observer to target

3. If the primary check fails due to obstacles or angle constraints, the system performs edge-to-edge visibility checks between the observer's and target's box colliders

4. Detected targets are stored in a "targetsInRange" list for use by other functions

This system enables XCOM style tactical visibility where high height obstacles only blocks direct line of sight, similar to how Units in XCOM peek out of the edges of obtacles to maitain line of sight of enemies.

### Setup 
- Add "XCOM_FOV" to the character's root parent object
- Create a new empty child object with a Box Collider and set "Is Trigger" to true to avoid world object collision
- The root parent and the child with the "XCOM_FOV" script must be in different layers to avoid unwanted additions to the "targetsInRange" list
- In order to filter between unit teams player and enemy child objects must have different tags
