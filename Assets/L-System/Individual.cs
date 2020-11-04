using System;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Individual : IComparable<Individual>
{
	public float fitness {get;set;}
	public StringBuilder rule {get;set;}
	public Vector3 dimensions {get;set;}
	public int numPoints {get;set;}

	public Individual(){
		fitness = float.NegativeInfinity;
		rule = new StringBuilder();
		numPoints = 0;
		dimensions = Vector3.zero;
	}

	//Sorts the individual with highest fitness first
	public int CompareTo(Individual other){
		if(other.fitness > this.fitness)
			return -1;
		else if(other.fitness == this.fitness)
			return 0;
		else
			return 1;
	}
}