using System;

/*
 * Legend:
 * 
 * F: Forward
 * L: Yaw CClockwise
 * R: Yaw Clockwise
 * +: Roll Clockwise
 * -: roll CClockwise
 * U: Pitch up
 * D: pitch down
 * O: Increase the angle (obtuse)
 * A: Decrease the angle (acute)
 * B: Increase the spread
 * S: Decrease the spread
 * [ and ]: Begin and end branch
 */

/*
 * SubRules:
 * C: Curve
 * R: Room
 * I: Straight line
 */

//Each value needs to have the assigned symbol as a value, to get the evolution to work.
public enum SubRuleType	: byte{
	CURVE = (byte) 'C',
	ROOM = (byte) 'Q',
	LINE = (byte) 'I',
	HUMP = (byte) 'H',
	UTURN = (byte) 'T'
};

