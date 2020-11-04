using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TileFactory {
	



	public struct TileObj{
		public Vector3 tileID;// = Vector3.zero;
		public Vector3 tilePos;// = Vector3.zero;
		public Vector3 tileCentre;// = Vector3.zero;
		public List<LPoint> lPoints;
	};
	public static List<TileObj> tileList;
	public struct LPoint{
		public Vector3 lPoint;
		public Vector2 noise2;
		//public Vector3 noise3;
		public Vector4 noise3;
		public LPoint(Vector3 _lPoint, Vector2 _noise2){
			lPoint = _lPoint;
			noise2 = _noise2;
			noise3 = Vector4.zero;
		}
		public LPoint(Vector3 _lPoint, Vector4 _noise3){
			lPoint = _lPoint;
			noise3 = _noise3;
			noise2 = Vector2.zero;
		}
	}

	private struct PotentialNeighbour{
		public Vector3 tilePos;// = Vector3.zero;
		public Vector3 actualTileCentre;// = Vector3.zero;
		public PotentialNeighbour(Vector3 tilePos_, Vector3 actualTilePos_){
			tilePos = tilePos_;
			actualTileCentre = actualTilePos_;
		}
	};

	private static VoxelCubeManager stateManager;
	public static Action<Vector3> AddPointToATile = DoNothing;
	private static float tileWidth;
	private static float tileWidthHalf;
	private static float tileWidthCust;
	private static float tileOffset;
	private static float mod = 1;
	//private static float distToCorner = 0;
	private static float outerRadius = 0;

	//private static Dictionary<int, TileObj> tileListVTable =
	//	new Dictionary<int, TileObj> ();
	private static Dictionary<Vector3, TileObj> tileListVTable =
		new Dictionary<Vector3, TileObj> ();
	
	
	public static void Start(VoxelCubeManager _stateManager){
		stateManager = _stateManager;
		AddPointToATile = addPointToATileV3;
		tileList = new List<TileObj>();
		//tileWidth = (stateManager.cubeSize -7) * stateManager.cubeSpacing;//7//-3//-8
		//tileWidth = (stateManager.cubeSize -6) * stateManager.cubeSpacing;//7//-3//-8
		tileWidth = (stateManager.cubeSize -1) * stateManager.cubeSpacing;//7//-3//-8
		//tileWidthHalf = tileWidth/2;
		//tileWidthHalf = tileWidth*0.499f;
		//tileWidthHalf = tileWidth*0.45f;
		//tileWidthHalf = tileWidth*0.525f;
		//tileWidthHalf = tileWidth*0.65f;
		tileWidthHalf = tileWidth/2;//now gets centre



		//tileOffset = stateManager.cubeSize/2 *stateManager.cubeSpacing;
		//distToCorner = Mathf.Abs(Vector3.Distance(Vector3.one * (tileWidth/2), Vector3.one * tileWidth));
		outerRadius = stateManager.settings.sprayOrbRadius + (stateManager.settings.sprayOrbOuterRadius - stateManager.settings.sprayOrbRadius);// + 2;// /2;
		//outerRadius = stateManager.sprayOrbRadius + (stateManager.sprayOrbOuterRadius - stateManager.sprayOrbRadius/2);// + 2;// /2;

		outerRadius += tileWidthHalf;

		//tileWidthCust = tileWidthHalf + outerRadius;//*0.80f;//*0.65f;
		//tileWidthCust = tileWidthHalf/2+outerRadius;//*0.80f;//*0.65f;
		tileWidthCust = tileWidthHalf*0.5f+outerRadius;//*0.80f;//*0.65f;
		//0.8f
		mod = setMod();

		tileListVTable.Clear();
	}

	/*
		firstly, I am rounding to int. so (0.0, 0.1, 0.0) / 32, will give the same result
		secondly, the brush ain't moving

		do the correct lpoint size here, and remove the one in S2_LoadAndNoise, so it no longer has the 1.0 problem
		and it fits with /32

		Get spacing to be correct. aligned to the grid. otherwise automata won't run. (will not find neighbours)

	 * */
	/*
	private static void addPointToATile(Vector3 pt){
		pt = transformPoint(pt);
		Vector3 ret = new Vector3(pt.x, pt.y, pt.z);
		ret/=tileWidth;
		ret.x = Mathf.FloorToInt(ret.x);
		ret.y = Mathf.FloorToInt(ret.y);
		ret.z = Mathf.FloorToInt(ret.z);
		int i = 0;
		bool found = false;
		foreach(TileObj ob in tileList){
			if(Vector3.Equals(ob.tileID, ret)){
				ob.lPoints.Add(pt);
				found= true;
				Debug.Log(tileWidth+"--- Adding to existing tile at: "+ret+"; the point: "+pt+"; tiles: "+tileList.Count);
				break; 
			}
			i++;
		}

		if(!found){
			Debug.Log(tileWidth+"--- Creating a tile at: "+ret+"; from point: "+pt+"; tiles: "+tileList.Count);
			TileObj ob = new TileObj();
			ob.tileID = ret;
			ob.tilePos = ret*tileWidth;//*stateManager.cubeSpacing*stateManager.lSystemResolution;
			ob.lPoints = new List<Vector3>();
			ob.lPoints.Add(pt);
			tileList.Add(ob);
		}
	}*/

	//static int debugi = 0;
	private static void addPointToATileV3(Vector3 pt){
		pt = transformPoint(pt);
		Vector3 ret = new Vector3(pt.x, pt.y, pt.z);
		
		
		ret/=tileWidth;
		
		//Vector3 ret2 = new Vector3(pt.x, pt.y, pt.z);
		ret.x = Mathf.FloorToInt(ret.x);
		ret.y = Mathf.FloorToInt(ret.y);
		ret.z = Mathf.FloorToInt(ret.z);
		

		List<Vector3> neighbours = new List<Vector3>();

		for(int i = -1; i <= 1; i++){
			for(int j = -1; j <= 1; j++){
				for(int k = -1; k <= 1; k++){
					//Vector3 tmp = new Vector3(ret.x + i,ret.y + j,ret.z + k);
					//Vector3 tmp2 = tmp*tileWidth;
					//tmp2 = new Vector3(tmp2.x + tileWidthHalf, tmp2.y + tileWidthHalf, tmp2.z + tileWidthHalf);
					neighbours.Add(new Vector3(ret.x + i,ret.y + j,ret.z + k));

				}
			}
		}


		/*
		if(debugi<100){
			Debug.Log(ret);
			debugi++;
		}
		*/


		foreach(Vector3 ter in neighbours){
			//tileWidthOneAndHalf
			//if(ter

			//int i = 0;
			bool found = false;

			//Vector3 terRound = new Vector3(Mathf.Round(ter.x*1000)/1000, Mathf.Round(ter.y*1000)/1000, Mathf.Round(ter.z*1000)/1000);
			//int terhash = terRound.GetHashCode();

			TileObj testTile; 
			if( tileListVTable.TryGetValue(ter, out testTile)){
				//testTile= tileListVTable[ter];

				if(
					Mathf.Abs(pt.x - testTile.tileCentre.x) <= tileWidthCust &&
					Mathf.Abs(pt.y - testTile.tileCentre.y) <= tileWidthCust &&
					Mathf.Abs(pt.z - testTile.tileCentre.z) <= tileWidthCust
					){
					
					testTile.lPoints.Add(new LPoint(pt, makeNoise2(pt)));
				}
				found= true;
			}

			
			if(!found){
				//Debug.Log(tileWidth+"--- Creating a tile at: "+ter+"; from point: "+pt+"; tiles: "+tileList.Count);

				Vector3 actualTilePos;//actualTilePos.GetHashCode
				Vector3 actualTileCentre;
				//tmp2 = new Vector3(tmp2.x + tileWidthHalf, tmp2.y + tileWidthHalf, tmp2.z + tileWidthHalf);
				actualTilePos = ter * tileWidth;
				actualTileCentre = new Vector3(actualTilePos.x + tileWidthHalf, actualTilePos.y + tileWidthHalf, actualTilePos.z + tileWidthHalf);

				if(
					Mathf.Abs(pt.x - actualTileCentre.x) <= tileWidthCust &&
					Mathf.Abs(pt.y - actualTileCentre.y) <= tileWidthCust &&
					Mathf.Abs(pt.z - actualTileCentre.z) <= tileWidthCust
					){

					TileObj ob = new TileObj();
					ob.tileID = ter;
					ob.tilePos = actualTilePos;//*tileWidth;//*stateManager.cubeSpacing*stateManager.lSystemResolution;
					ob.tileCentre = actualTileCentre;
					ob.lPoints = new List<LPoint>();
					ob.lPoints.Add(new LPoint(pt, makeNoise2(pt)));

					//if(tileList.Count <1555){//malloc overflow otherwise. Too lazy to swap or something
						tileList.Add(ob);
						tileListVTable.Add(ter, ob);
					//}
					//else{
					//	AddPointToATile = DoNothing;
					//}
				}
			}
		}
	}

	private static void addPointToATileV2(Vector3 pt){
		pt = transformPoint(pt);
		Vector3 ret = new Vector3(pt.x, pt.y, pt.z);


		ret/=tileWidth;

		//Vector3 ret2 = new Vector3(pt.x, pt.y, pt.z);
		ret.x = Mathf.FloorToInt(ret.x);
		ret.y = Mathf.FloorToInt(ret.y);
		ret.z = Mathf.FloorToInt(ret.z);


		//TODO: note than now ALL points from one cube are mirrored in all its neighbours.
		//this works fine, because if they are out of bounds they're not drawn, but you waste time.
		//therefore TODO: only include a pt in neighbours if it is OuterRadius away from an edge.
		//get dist between a centre of cube, and a corner. (in start)
		//in for loops, only add a point to a tile if dist(pt, centreOfCurrentTile) < dist^ + outerrad
		//UPDATE: Done!
		List<Vector3> neighbours = new List<Vector3>();

		Vector3 acTilePos = ret*tileWidth;
		//Vector3 acTilePosEnd = new Vector3(acTilePos.x, acTilePos.y, acTilePos.z);
		Vector3 acTilePosOrb = new Vector3(acTilePos.x, acTilePos.y, acTilePos.z);
		Vector3 acTilePosEndOrb = new Vector3(acTilePos.x, acTilePos.y, acTilePos.z);
				
				acTilePosOrb.x += outerRadius;
				acTilePosOrb.y += outerRadius;
				acTilePosOrb.z += outerRadius;

				acTilePosEndOrb.x += tileWidth - outerRadius;
				acTilePosEndOrb.y += tileWidth - outerRadius;
				acTilePosEndOrb.z += tileWidth - outerRadius;


		if(acTilePosOrb.x <= pt.x && pt.x <= acTilePosEndOrb.x &&
		   acTilePosOrb.y <= pt.y && pt.y <= acTilePosEndOrb.y &&
		   acTilePosOrb.z <= pt.z && pt.z <= acTilePosEndOrb.z ){
			//Debug.Log("acTilePos ["+acTilePosOrb+"] < pt ["+pt+"] < acTilePosEnd ["+acTilePosEndOrb+"]");
			neighbours.Add(new Vector3(
				ret.x,
				ret.y,
				ret.z
				/*
				Mathf.FloorToInt(ret.x),
				Mathf.FloorToInt(ret.y),
				Mathf.FloorToInt(ret.z)
				*/
				));
		}
		else
		//only do these fors if the current point is within OuterRad away from the current cube's bounds
		for(int i = -1; i <= 1; i++){
			for(int j = -1; j <= 1; j++){
				for(int k = -1; k <= 1; k++){
					
					//TODO: even here, don't add all neighbours. only add it in the direction you were.
					//OTHERWISE, you will have all the outliers in all neighbours. better, but still suboptimal
					//if(!(i==0 && j==0 && k==0))
					/*
					if(
						acTilePos.x < pt.x && pt.x < acTilePosOrb.x &&
						acTilePos.y < pt.y && pt.y < acTilePosOrb.y &&
						acTilePos.z < pt.z && pt.z < acTilePosOrb.z
						)
						*/
					Vector3 val = new Vector3(
									ret.x + i,
									ret.y + j,
									ret.z + k
									/*
										Mathf.FloorToInt(ret.x) + i,
										Mathf.FloorToInt(ret.y) + j,
										Mathf.FloorToInt(ret.z) + k
										*/
									);
					Vector3 tempTilePosCentre = val * tileWidth;
					tempTilePosCentre.x += tileWidthHalf;
					tempTilePosCentre.y += tileWidthHalf;
					tempTilePosCentre.z += tileWidthHalf;
					//if(Mathf.Abs(Vector3.Distance(pt, tempTilePosCentre)) < distToCorner+outerRadius)
					//	neighbours.Add(val);
					if(Mathf.Abs(pt.x- tempTilePosCentre.x) <= tileWidthHalf+outerRadius &&
					   Mathf.Abs(pt.y- tempTilePosCentre.y) <= tileWidthHalf+outerRadius &&
					   Mathf.Abs(pt.z- tempTilePosCentre.z) <= tileWidthHalf+outerRadius
					   )
						neighbours.Add(val);
				}
			}
		}

		/*
		neighbours.Add(new Vector3(
			Mathf.FloorToInt(ret.x),
			Mathf.FloorToInt(ret.y),
			Mathf.FloorToInt(ret.z)
			));
		neighbours.Add(new Vector3(
			Mathf.CeilToInt(ret.x),
			Mathf.FloorToInt(ret.y),
			Mathf.FloorToInt(ret.z)
			));
		neighbours.Add(new Vector3(
			Mathf.FloorToInt(ret.x),
			Mathf.CeilToInt(ret.y),
			Mathf.FloorToInt(ret.z)
			));
		neighbours.Add(new Vector3(
			Mathf.FloorToInt(ret.x),
			Mathf.FloorToInt(ret.y),
			Mathf.CeilToInt(ret.z)
			));
		neighbours.Add(new Vector3(
			Mathf.CeilToInt(ret.x),
			Mathf.CeilToInt(ret.y),
			Mathf.FloorToInt(ret.z)
			));
		neighbours.Add(new Vector3(
			Mathf.CeilToInt(ret.x),
			Mathf.FloorToInt(ret.y),
			Mathf.CeilToInt(ret.z)
			));
		neighbours.Add(new Vector3(
			Mathf.FloorToInt(ret.x),
			Mathf.CeilToInt(ret.y),
			Mathf.CeilToInt(ret.z)
			));
		neighbours.Add(new Vector3(
			Mathf.CeilToInt(ret.x),
			Mathf.CeilToInt(ret.y),
			Mathf.CeilToInt(ret.z)
			));
		*/


		foreach(Vector3 ter in neighbours){

			int i = 0;
			bool found = false;
			foreach(TileObj ob in tileList){
				if(Vector3.Equals(ob.tileID, ter)){
					ob.lPoints.Add(new LPoint(pt, makeNoise2(pt)));
					found= true;
					//Debug.Log(tileWidth+"--- Adding to existing tile at: "+ter+"; the point: "+pt+"; tiles: "+tileList.Count);
					break; 
				}
				i++;
			}
			
			if(!found){
				//Debug.Log(tileWidth+"--- Creating a tile at: "+ter+"; from point: "+pt+"; tiles: "+tileList.Count);
				TileObj ob = new TileObj();
				ob.tileID = ter;
				ob.tilePos = ter*tileWidth;//*stateManager.cubeSpacing*stateManager.lSystemResolution;
				ob.lPoints = new List<LPoint>();
				ob.lPoints.Add(new LPoint(pt, makeNoise2(pt)));
				tileList.Add(ob);
			}
		}
	}

	private static Vector4 makeNoise2(Vector3 point){
		//point/= 55;
		//point*= 10;

		//Vector2 result = Vector2.zero;
		//result.x = SimplexNoise.Noise.Generate(point.x/55, point.z/55);//50 //25 is larger scale changes; lower freq
		//result.y = SimplexNoise.Noise.Generate(point.y);

		//float freq = 0.005f;
		float freq = 0.003f;
		//float result2 = SimplexNoise.Noise.Generate(point.x*freq, point.y*freq, point.z*freq)*(stateManager.fineDetailYSlope*2);
		float result2 = SimplexNoise.Noise.Generate(point.x*freq, point.y*freq, point.z*freq)*4.8f;//5.45f;
		float result3 = result2;
		if(result2<0){
			result2=0;
		}

		//Vector3 result = SimplexNoise.Noise.get_Curl(new Vector3(point.x/55, point.y*800, point.z/55));
		//Vector3 result = SimplexNoise.Noise.get_Curl(new Vector3(point.x/55, point.y/5, point.z/55))*1.5f;
		//Vector3 result = SimplexNoise.Noise.get_Curl(new Vector3(point.x/55, point.y, point.z/55));
		Vector4 result = SimplexNoise.Noise.get_Curl(new Vector4(point.x, point.y, point.z,1));//*0.7f;

		//Vector3 result = SimplexNoise.Noise.get_Curl(new Vector3(point.x/55, point.y, point.z/55));
		//result.x = res.x;
		//result.y = res.y;

		/*
		mapping [A, B] to [var, 1]
		yourVal =  ((yourVal - A) / (B - A)) * (1 - var) + var;
		*/
		//float min = 0.82f;//0.8//.75f;//0.6f;//0.5f;
		//float max = 1.18f;//0.2//1.25f;//1.2f;//1.5f;
		float minY = 1 - stateManager.fineDetailYSlope;//  +(result2<0?0:result2);
		float maxY = 1 + stateManager.fineDetailYSlope;//  +(result2>0?0:result2);
		//result.x = ((result.x +1) / 2) * (maxY- minY) + minY;
		result.x = ((result.x + result2 ) / 7) * (maxY- minY) + minY;
		//result.y = 1;//(((result.y+/2 +1) / 2) * (maxY- minY) + minY)*1.25f;
		//result.z = ((result.z +1) / 2) * (maxY- minY) + minY;
		result.z = ((result.y + result2 ) / 7) * (maxY- minY) + minY;
		//result.y = ((1/result.x +1) / 2) * (max- min) + min;//((result.y +1) / 2) * (max- min) + min;


		//result.y = 1.27f -result3/9f;//(((result.y+/2 +1) / 2) * (maxY- minY) + minY)*1.25f;
		//result.y = 1.14f -result3/10f;//(((result.y+/2 +1) / 2) * (maxY- minY) + minY)*1.25f;
		result.y = 1f -result3/10f;//(((result.y+/2 +1) / 2) * (maxY- minY) + minY)*1.25f;



		//if(result.y<1){
		//if(result.y <1.23f){
			//result.x = (result.x + 3f)/4f;
			//result.z = (result.z + 3f)/4f;
			//result.x *= 2;
			//result.z *= 2;
			//result.y -= -0.4f;
		//}
		//else{ //if(result.y >1.27f){//1.4
			//result.x = (result.x + 2f)/3f;
			//result.z = (result.z + 2f)/3f;
		//}


		result.w = (result.x*result.y*result.z);///20;



		return result;
	}

	private static Vector3 transformPoint(Vector3 pt){
		pt*= stateManager.lSystemResolution;

		pt = new Vector3(pt.x+mod, pt.y+mod, pt.z);

		return pt;
	}

	private static float setMod(){
		return (stateManager.cubeSize/2)* stateManager.cubeSpacing;
	}

	private static void DoNothing(Vector3 point){
	}

	/*
	static TileFactory instance = null;
	public static TileFactory Inst{
		get{
			if(instance == null){
				instance = new TileFactory();
			}
			return instance;
		}
	}
	*/
}
