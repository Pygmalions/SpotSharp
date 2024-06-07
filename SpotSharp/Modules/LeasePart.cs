namespace SpotSharp.Modules;

public enum LeasePart
{
    /// <summary>
    /// Body contains FullArm and Mobility.
    /// </summary>
    Body = 1,
    /// <summary>
    /// FullArm contains Gripper and Arm.
    /// </summary>
    FullArm = 11,
    Mobility = 12,
    Gripper = 111,
    Arm = 112
}