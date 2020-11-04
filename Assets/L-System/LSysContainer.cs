using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("LSystem")]
public class LSysContainer
{
	public string axiom = "";
	public int spread = 20;
	public int angle = 45;
	public string initialRule = "";

	[XmlArray("RandomSeed"), XmlArrayItem("Seed")]
	public float[] randomSeed = new float[3];

	[XmlArray("Connections"), XmlArrayItem("Connection")]
	public List<LSysLine> connections = new List<LSysLine>();

	public void save(string path){
		XmlSerializer serializer = new XmlSerializer(typeof(LSysContainer));
		using(FileStream stream = new FileStream(path, FileMode.Create)){
			serializer.Serialize(stream, this);
		}
	}

	public static LSysContainer load(string path){
		XmlSerializer serializer = new XmlSerializer(typeof(LSysContainer));
		using(FileStream stream = new FileStream(path, FileMode.Open)){
			return serializer.Deserialize(stream) as LSysContainer;
		}
	}
}

