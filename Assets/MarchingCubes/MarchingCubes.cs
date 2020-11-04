using UnityEngine;
using System.Collections;
using System.Collections.Generic;
	
public class MarchingCubes : MonoBehaviour
{
	//Function delegates, makes using functions pointers easier
	delegate void MODE_FUNC(Vector3 pos, float[] cube, List<Vector3> vertList, List<int> indexList, int cubeID, VoxelCubeManager.PPParticle[] ppVoxels, Vector3 voxPos, bool isCubeEdge);
	//Function poiter to what mode to use, cubes or tetrahedrons
	MODE_FUNC Mode_Func = MarchCube;
	//Set the mode to use
	//Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface
	public void SetModeToCubes() { Mode_Func = MarchCube; }
	public void SetModeToTetrahedrons() { Mode_Func = MarchCubeTetrahedron; } 
	
	public static void SetTarget(float tar) { target = tar; }
	public static void SetWindingOrder(int v0, int v1, int v2) { windingOrder = new int[]{ v0, v1, v2 }; }

	/*
	struct VoxelParticle
	{
		public Vector3 position;
		public Vector3 velocity;
		public float noise;
		public int flags;
		//public Vector3[] cubeVerts;
	};*/

	VoxelCubeManager particleCube;
	
	void Start(){
		particleCube = gameObject.GetComponent<VoxelCubeManager>();
	}

	//private static VoxelCubeManager.PPParticle[] ppVoxels;

	public static int buff = 0;//1;//1//3;//1;//3;
	public static int buff2 = 0;//1;//2
	private int low;
	private int high;
	private int l;
	private int h;
	public Mesh CreateMeshFromList(VoxelCubeManager.VoxelParticle[] voxels, VoxelCubeManager.PPParticle[] ppVoxels)
	{
		//ppVoxels = _ppVoxels;
		//ppVoxels = new VoxelCubeManager.PPParticle[particleCube.particleCount];

		low = -1 + buff;
		high = particleCube.cubeSize - buff-1;
		l = low+buff2;
		h = high-buff2;
		
		List<Vector3> verts = new List<Vector3>();
		//List<Vector3> norms = new List<Vector3>();
		List<int> index = new List<int>();
		
		float[] cube = new float[8];
		bool isCubeEdge = false;
		float avgNoise = 0;

		if(particleCube.ClipNrOfVerts0_to_1 > 1.0f)
			particleCube.ClipNrOfVerts0_to_1 = 1.0f;
		else
			if(particleCube.ClipNrOfVerts0_to_1 < 0.1f)
				particleCube.ClipNrOfVerts0_to_1 = 0.1f;
		for(int i = 0; i < voxels.Length * particleCube.ClipNrOfVerts0_to_1; i++)//-1
		//for(int i = 0; i < 5000; i++)//-1 //4000
		{

			VoxelCubeManager.VoxelParticle p = (VoxelCubeManager.VoxelParticle)voxels[i];


			//if(p.noise >=-1){// && p.noise <=1){
			if(p.noise >=-10){

				//TODO: remove this
				//if(tempGlobalDebugLimit > 0 ){
					//Debug.Log("i= "+i+"; p.position: "+p.position);
					//tempGlobalDebugLimit--;
				//}

				int[] cubeIDs = {
											(int)((p.position.x-TileFactory.tileList[particleCube.currentTile].tilePos.x)/particleCube.cubeSpacing),
											(int)((p.position.y-TileFactory.tileList[particleCube.currentTile].tilePos.y)/particleCube.cubeSpacing),
											(int)((p.position.z-TileFactory.tileList[particleCube.currentTile].tilePos.z)/particleCube.cubeSpacing)
								};


				//Get the values in the 8 neighbours which make up a cube
				bool success = FillCubeList(cubeIDs[0],
				                            cubeIDs[1],
				                            cubeIDs[2],
				                            voxels,cube, out isCubeEdge, out avgNoise);
				ppVoxels[i] = new VoxelCubeManager.PPParticle();
				ppVoxels[i].avgNoise = avgNoise;

				if(success){
					//Perform algorithm
					Mode_Func(p.position/particleCube.cubeSpacing, cube, verts, index, coordsToIndex(cubeIDs[0], cubeIDs[1], cubeIDs[2]), ppVoxels, p.position, isCubeEdge);
				}
			}
		}

		//Debug.Log ("Marching cubes - about to return mesh. [backend]");

		Mesh mesh = new Mesh();
		
		mesh.vertices = verts.ToArray();		
		mesh.triangles = index.ToArray();
		//mesh.normals = norms.ToArray();

		/*
		if(true){
			//once = false;
			for(int u=0; u< 32*32*10; u++){
				if(ppVoxels[u] != null){
					//for(int e = 0; e< ppVoxels[u].verts.Length; e++){
						//Debug.Log(u+"; !$%@#%#$%#$% "+ppVoxels[u].verts[e]);
						Debug.Log(u+"; !$%@#%#$%#$% "+ppVoxels[u].verts);
					//}
				}
			}
		}
		*/
		//Debug.Log("THE GENERATED MESH HAS "+mesh.vertices.Length+" VERTICES, "+mesh.triangles.Length+" triangles. Max in Unity is 65000. If you get an error, you should set the particleCube.ClipNrOfVerts0_to_1 to 0.5 (or something < 1) in the Inspector. Note that \"marchingTetrahedrons\" uses far more vertices than the marchingCubes but is more precise.");
		#if UNITY_EDITOR
		Debug.Log("THE GENERATED MESH HAS "+mesh.vertices.Length+" VERTICES, "+mesh.triangles.Length+" triangles. Max in Unity is 65000.");
		#endif
		//particleCube.postProcessArray = ppVoxels;

		return mesh;
	}
	/*
	public Mesh CreateMesh(float[,,] voxels)
	{

		List<Vector3> verts = new List<Vector3>();
		List<Vector3> norms = new List<Vector3>();
		List<int> index = new List<int>();
		
		float[] cube = new float[8];
		
		for(int x = 0; x < voxels.GetLength(0)-1; x++)
		{
			for(int y = 0; y < voxels.GetLength(1)-1; y++)
			{
				for(int z = 0; z < voxels.GetLength(2)-1; z++)
				{
					//Get the values in the 8 neighbours which make up a cube
					FillCube(x,y,z,voxels,cube);
					//Perform algorithm
					Mode_Func(new Vector3(x,y,z), cube, verts, index, norms);
				}
			}
		}
		
		Mesh mesh = new Mesh();

		mesh.vertices = verts.ToArray();		
		mesh.triangles = index.ToArray();
		mesh.normals = norms.ToArray();

		Debug.Log("THE GENERATED MESH HAS "+mesh.vertices.Length+" VERTICES. Max in Unity is 65000.");

		return mesh;
	}
*/
	//TODO: remove this:
	//int tempGlobalDebugLimit2 = 70;
	//int tempGlobalDebugLimit = 0;
	int coordsToIndex(float x, float y,float z){
		//Flat[x + WIDTH * (y + DEPTH * z)] = Original[x, y, z]
		//int s = 4;
		int index = (int)(x + particleCube.cubeSize * (z + particleCube.cubeSize * y));
		/*
		if(index < 0){// || index > particleCube.cubeSize*particleCube.cubeSize*particleCube.cubeSize ){

			if(tempGlobalDebugLimit2 > 0){
				Debug.Log ("OUT OF RANGE: "+index+"; pos: ["+x+"]["+y+"]["+z+"];");
				tempGlobalDebugLimit2--;
			}
			return 0;
		}
		else if(x < particleCube.transform.position.x +TileFactory.tileList[particleCube.currentTile].tilePos.x || x > particleCube.transform.position.x + particleCube.cubeSize-s +TileFactory.tileList[particleCube.currentTile].tilePos.x||
		        y < particleCube.transform.position.y +TileFactory.tileList[particleCube.currentTile].tilePos.y || y > particleCube.transform.position.y + particleCube.cubeSize-s +TileFactory.tileList[particleCube.currentTile].tilePos.y||
		        z < particleCube.transform.position.z +TileFactory.tileList[particleCube.currentTile].tilePos.z || z > particleCube.transform.position.z + particleCube.cubeSize-s +TileFactory.tileList[particleCube.currentTile].tilePos.z 
		        ){

			return -1;
		}
		else*/
			return index;
	}


	bool FillCubeList(int x, int y, int z, VoxelCubeManager.VoxelParticle[] voxels, float[] cube, out bool isCubeEdge, out float avgNoise)
	{
		avgNoise = 0;
		bool success = true;
		isCubeEdge = false;
		if(x>low && x<high && y>low && y<high && z>low && z<high){

			if(x<=l || x>=h || y<=l || y>=h || z<=l || z>=h){
				isCubeEdge = true;
			}

			for(int i = 0; i < 8; i++){
				/*
				if(tempGlobalDebugLimit <= 262144 && tempGlobalDebugLimit >= 262044){//262144*0.0001f-100
					Debug.Log("?INDEX: i:"+i+"; coords to index: "+coordsToIndex(x + vertexOffset[i,0],
					                        y + vertexOffset[i,1], 
					                        z + vertexOffset[i,2])
					          +"; voxels.Length: "+voxels.Length+"; tempGlobalDebugLimit: "+tempGlobalDebugLimit);

				}
				tempGlobalDebugLimit++;
				*/

				int coord = coordsToIndex(x + vertexOffset[i,0],
				                          y + vertexOffset[i,1], 
				                          z + vertexOffset[i,2]);
				//Debug.Log(x+"; "+y+"; "+z+"; calcd: "+coord+"; voxels.len: "+voxels.Length);

				if(coord > -1 && coord < voxels.Length){
					/*
					if(voxels[coord].noise>0){
						//voxels[coord].noise = 1;
						voxels[coord].noise +=0.25f;
					}
					else{
						//voxels[coord].noise = -1;
						voxels[coord].noise -=0.25f;
					}
					*/
					//voxels[coord].noise = Mathf.Round(voxels[coord].noise*50) /50;


					//cube[i] = voxels[coord].noise <0? voxels[coord].noise - 0.019f : voxels[coord].noise + 0.019f;
					cube[i] = voxels[coord].noise;
					avgNoise += voxels[coord].noise;

				}
				else {


					success = false;
					break;
				}

			}
			avgNoise /= 8;

			return success;
		}
		return false;
	
	}

	void FillCube(int x, int y, int z, float[,,] voxels, float[] cube)
	{
		for(int i = 0; i < 8; i++)
			cube[i] = voxels[x + vertexOffset[i,0], y + vertexOffset[i,1], z + vertexOffset[i,2]];
	}
	
	// GetOffset finds the approximate point of intersection of the surface
	// between two points with the values v1 and v2
	static float GetOffsetOrig(float v1, float v2)
	{
	    float delta = v2 - v1;
	    return (delta == 0.0f) ? 0.5f : (target - v1)/delta;
		//target is the 0 value
	}
	static float GetOffset(float v1, float v2)
	{
		float delta = v2 - v1;
		return (delta == 0.0f) ? 0.5f : (target - v1)/delta;
		//target is the 0 value
	}
	/*
	static int thresh = 4;
	static float GetOffset(float v0, float v1, int thresh){
		float v = v1 - v0;
		if(v<thresh){

		}
	}
	*/

	//MarchCube performs the Marching Cubes algorithm on a single cube
	static void MarchCube(Vector3 pos, float[] cube, List<Vector3> vertList, List<int> indexList, int cubeID, VoxelCubeManager.PPParticle[] ppVoxels, Vector3 voxPos, bool isCubeEdge)
	{

		if(!isCubeEdge){
			int i, j, vert, idx;
			int flagIndex = 0;
			float offset = 0.0f;
			
		    Vector3[] edgeVertex = new Vector3[12];
		
		    //Find which vertices are inside of the surface and which are outside
		    for(i = 0; i < 8; i++) if(cube[i] <= target) flagIndex |= 1<<i;
		
		    //Find which edges are intersected by the surface
		    int edgeFlags = cubeEdgeFlags[flagIndex];
		
		    //If the cube is entirely inside or outside of the surface, then there will be no intersections
		    if(edgeFlags == 0) return;
		

		    //Find the point of intersection of the surface with each edge
		    for(i = 0; i < 12; i++)
		    {
		        //if there is an intersection on this edge
		        if((edgeFlags & (1<<i)) != 0)
		        {
		         	offset = GetOffset(cube[edgeConnection[i,0]], cube[edgeConnection[i,1]]);
					//offset is between 0.0x and 0.9x

	                edgeVertex[i].x = pos.x + (vertexOffset[edgeConnection[i,0],0] + offset * edgeDirection[i,0]);
	                edgeVertex[i].y = pos.y + (vertexOffset[edgeConnection[i,0],1] + offset * edgeDirection[i,1]);
	                edgeVertex[i].z = pos.z + (vertexOffset[edgeConnection[i,0],2] + offset * edgeDirection[i,2]);
		        	

				}
		    }
		

		

			//List<Vector3> voxelVert = new List<Vector3>();
			List<Vector3> voxelNorm = new List<Vector3>();
			//List<int> trianglist = new List<int>();
		    //Save the triangles that were found. There can be up to five per cube
		    for(i = 0; i < 5; i++)
		    {
	            if(triangleConnectionTable[flagIndex,3*i] < 0) break;
				
				idx = vertList.Count;

	            for(j = 0; j < 3; j++)
	            {
	                vert = triangleConnectionTable[flagIndex,3*i+j];
					indexList.Add(idx+windingOrder[j]);
					vertList.Add((edgeVertex[vert]));

					//voxelVert.Add(edgeVertex[vert]);
					//trianglist.Add(idx+windingOrder[j]);
	            }
				calculateNormal(vertList, voxelNorm);
		    }
			//ppVoxels[cubeID] = new VoxelCubeManager.PPParticle();
			ppVoxels[cubeID].position = voxPos;
			//ppVoxels[cubeID].verts = voxelVert.ToArray();
			ppVoxels[cubeID].norms = voxelNorm.ToArray();       //TODO 

			//ppVoxels[cubeID].tris = trianglist.ToArray();       //TODO 
			ppVoxels[cubeID].used = true;							//TODO:
		}
		ppVoxels[cubeID].isEdge = isCubeEdge;
	}

	static void calculateNormal(List<Vector3> vertList, List<Vector3> voxelNorm){

		Vector3 side1 = vertList[vertList.Count-2] - vertList[vertList.Count-1];
		Vector3 side2 = vertList[vertList.Count-3] - vertList[vertList.Count-1];
		Vector3 norm = Vector3.Cross(side1, side2);
		norm /= norm.magnitude;
		for(int q=0; q<3; q++){
			//normList.Add(norm);//TODO: use this if you want to assign the straight normals right away to the mesh.
			voxelNorm.Add(norm);
		}

	}
	
	//MarchTetrahedron performs the Marching Tetrahedrons algorithm on a single tetrahedron
	static void MarchTetrahedron(Vector3[] tetrahedronPosition, float[] tetrahedronValue, List<Vector3> vertList, List<int> indexList, int cubeID, VoxelCubeManager.PPParticle[] ppVoxels, Vector3 voxPos, bool isCubeEdge)
	{
		int i, j, vert, vert0, vert1, idx;
		int flagIndex = 0, edgeFlags;
		float offset, invOffset;
		
		Vector3[] edgeVertex = new Vector3[6];
	
	    //Find which vertices are inside of the surface and which are outside
	    for(i = 0; i < 4; i++) if(tetrahedronValue[i] <= target) flagIndex |= 1<<i;
	
	    //Find which edges are intersected by the surface
	    edgeFlags = tetrahedronEdgeFlags[flagIndex];
	
	    //If the tetrahedron is entirely inside or outside of the surface, then there will be no intersections
	    if(edgeFlags == 0) return;

	    //Find the point of intersection of the surface with each edge
	    for(i = 0; i < 6; i++)
	    {
            //if there is an intersection on this edge
            if((edgeFlags & (1<<i)) != 0)
            {
                vert0 = tetrahedronEdgeConnection[i,0];
                vert1 = tetrahedronEdgeConnection[i,1];
                offset = GetOffset(tetrahedronValue[vert0], tetrahedronValue[vert1]);
                invOffset = 1.0f - offset;

                edgeVertex[i].x = invOffset*tetrahedronPosition[vert0].x + offset*tetrahedronPosition[vert1].x;
                edgeVertex[i].y = invOffset*tetrahedronPosition[vert0].y + offset*tetrahedronPosition[vert1].y;
                edgeVertex[i].z = invOffset*tetrahedronPosition[vert0].z + offset*tetrahedronPosition[vert1].z;     
            }
	    }

		//List<Vector3> voxelVert = new List<Vector3>();
		List<Vector3> voxelNorm = new List<Vector3>();
		//List<int> trianglist = new List<int>();
	    //Save the triangles that were found. There can be up to 2 per tetrahedron
	    for(i = 0; i < 2; i++)
	    {
            if(tetrahedronTriangles[flagIndex,3*i] < 0) break;
		
			idx = vertList.Count;

            for(j = 0; j < 3; j++)
            {
                vert = tetrahedronTriangles[flagIndex,3*i+j];
				indexList.Add(idx+windingOrder[j]);
				vertList.Add(edgeVertex[vert]);

				//voxelVert.Add(edgeVertex[vert]);
				//trianglist.Add(idx+windingOrder[j]);
            }
			calculateNormal(vertList, voxelNorm);
	    }
		//ppVoxels[cubeID] = new VoxelCubeManager.PPParticle();
		ppVoxels[cubeID].position = voxPos;
		//ppVoxels[cubeID].verts = voxelVert.ToArray();
		ppVoxels[cubeID].norms = voxelNorm.ToArray();
		ppVoxels[cubeID].isEdge = isCubeEdge;
		//ppVoxels[cubeID].tris = trianglist.ToArray(); 
		ppVoxels[cubeID].used = true;
	}
	
	//MarchCubeTetrahedron performs the Marching Tetrahedrons algorithm on a single cube
	static void MarchCubeTetrahedron(Vector3 pos, float[] cube, List<Vector3> vertList, List<int> indexList, int cubeID, VoxelCubeManager.PPParticle[] ppVoxels, Vector3 voxPos, bool isCubeEdge)
	{
		int i, j, vertexInACube;
		Vector3[] cubePosition = new Vector3[8];
		Vector3[] tetrahedronPosition = new Vector3[4];
		float[] tetrahedronValue = new float[4];
		
		//Make a local copy of the cube's corner positions
		for(i = 0; i < 8; i++) cubePosition[i] = new Vector3( pos.x + vertexOffset[i,0], pos.y + vertexOffset[i,1], pos.z + vertexOffset[i,2]);
		
		for(i = 0; i < 6; i++)
		{
	        for(j = 0; j < 4; j++)
	        {
                vertexInACube = tetrahedronsInACube[i,j];
                tetrahedronPosition[j] = cubePosition[vertexInACube];
                tetrahedronValue[j] = cube[vertexInACube];
	        }
			
			MarchTetrahedron(tetrahedronPosition, tetrahedronValue, vertList, indexList, cubeID, ppVoxels, voxPos, isCubeEdge);
		}
	}
	
	//Target is the value that represents the surface of mesh
	//For example a range of -1 to 1, 0 would be the mid point were we want the surface to cut through
	//The target value does not have to be the mid point it can be any value with in the range
	static float target = 0.5f;

	//Winding order of triangles use 2,1,0 or 0,1,2
	public static int[] windingOrder = new int[] { 0, 1, 2 };
	
	// vertexOffset lists the positions, relative to vertex0, of each of the 8 vertices of a cube
	// vertexOffset[8][3]
	
	static int[,] vertexOffset = new int[,]
	{
	    {0, 0, 0},{1, 0, 0},{1, 1, 0},{0, 1, 0},
	    {0, 0, 1},{1, 0, 1},{1, 1, 1},{0, 1, 1}
	};

	// edgeConnection lists the index of the endpoint vertices for each of the 12 edges of the cube
	// edgeConnection[12][2]
	
	static int[,] edgeConnection = new int[,] 
	{
	    {0,1}, {1,2}, {2,3}, {3,0},
	    {4,5}, {5,6}, {6,7}, {7,4},
	    {0,4}, {1,5}, {2,6}, {3,7}
	};

	// edgeDirection lists the direction vector (vertex1-vertex0) for each edge in the cube
	// edgeDirection[12][3]
	
	static float[,] edgeDirection = new float[,]
	{
	    {1.0f, 0.0f, 0.0f},{0.0f, 1.0f, 0.0f},{-1.0f, 0.0f, 0.0f},{0.0f, -1.0f, 0.0f},
	    {1.0f, 0.0f, 0.0f},{0.0f, 1.0f, 0.0f},{-1.0f, 0.0f, 0.0f},{0.0f, -1.0f, 0.0f},
	    {0.0f, 0.0f, 1.0f},{0.0f, 0.0f, 1.0f},{ 0.0f, 0.0f, 1.0f},{0.0f,  0.0f, 1.0f}
	};

	// tetrahedronEdgeConnection lists the index of the endpoint vertices for each of the 6 edges of the tetrahedron
	// tetrahedronEdgeConnection[6][2]
	
	static int[,] tetrahedronEdgeConnection = new int[,]
	{
	    {0,1},  {1,2},  {2,0},  {0,3},  {1,3},  {2,3}
	};

	// tetrahedronEdgeConnection lists the index of verticies from a cube 
	// that made up each of the six tetrahedrons within the cube
	// tetrahedronsInACube[6][4]
	
	static int[,] tetrahedronsInACube = new int[,]
	{
	    {0,5,1,6},
	    {0,1,2,6},
	    {0,2,3,6},
	    {0,3,7,6},
	    {0,7,4,6},
	    {0,4,5,6}
	};
	
	// For any edge, if one vertex is inside of the surface and the other is outside of the surface
	//  then the edge intersects the surface
	// For each of the 4 vertices of the tetrahedron can be two possible states : either inside or outside of the surface
	// For any tetrahedron the are 2^4=16 possible sets of vertex states
	// This table lists the edges intersected by the surface for all 16 possible vertex states
	// There are 6 edges.  For each entry in the table, if edge #n is intersected, then bit #n is set to 1
	// tetrahedronEdgeFlags[16]

	static int[] tetrahedronEdgeFlags = new int[]
	{
		0x00, 0x0d, 0x13, 0x1e, 0x26, 0x2b, 0x35, 0x38, 0x38, 0x35, 0x2b, 0x26, 0x1e, 0x13, 0x0d, 0x00
	};


	// For each of the possible vertex states listed in tetrahedronEdgeFlags there is a specific triangulation
	// of the edge intersection points.  tetrahedronTriangles lists all of them in the form of
	// 0-2 edge triples with the list terminated by the invalid value -1.
	// tetrahedronTriangles[16][7]

	static int[,] tetrahedronTriangles = new int[,]
	{
        {-1, -1, -1, -1, -1, -1, -1},
        { 0,  3,  2, -1, -1, -1, -1},
        { 0,  1,  4, -1, -1, -1, -1},
        { 1,  4,  2,  2,  4,  3, -1},

        { 1,  2,  5, -1, -1, -1, -1},
        { 0,  3,  5,  0,  5,  1, -1},
        { 0,  2,  5,  0,  5,  4, -1},
        { 5,  4,  3, -1, -1, -1, -1},

        { 3,  4,  5, -1, -1, -1, -1},
        { 4,  5,  0,  5,  2,  0, -1},
        { 1,  5,  0,  5,  3,  0, -1},
        { 5,  2,  1, -1, -1, -1, -1},

        { 3,  4,  2,  2,  4,  1, -1},
        { 4,  1,  0, -1, -1, -1, -1},
        { 2,  3,  0, -1, -1, -1, -1},
        {-1, -1, -1, -1, -1, -1, -1}
	};

	// For any edge, if one vertex is inside of the surface and the other is outside of the surface
	//  then the edge intersects the surface
	// For each of the 8 vertices of the cube can be two possible states : either inside or outside of the surface
	// For any cube the are 2^8=256 possible sets of vertex states
	// This table lists the edges intersected by the surface for all 256 possible vertex states
	// There are 12 edges.  For each entry in the table, if edge #n is intersected, then bit #n is set to 1
	// cubeEdgeFlags[256]

	static int[] cubeEdgeFlags = new int[]
	{
		0x000, 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c, 0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00, 
		0x190, 0x099, 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c, 0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90, 
		0x230, 0x339, 0x033, 0x13a, 0x636, 0x73f, 0x435, 0x53c, 0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30, 
		0x3a0, 0x2a9, 0x1a3, 0x0aa, 0x7a6, 0x6af, 0x5a5, 0x4ac, 0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0, 
		0x460, 0x569, 0x663, 0x76a, 0x066, 0x16f, 0x265, 0x36c, 0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60, 
		0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0x0ff, 0x3f5, 0x2fc, 0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0, 
		0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x055, 0x15c, 0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950, 
		0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0x0cc, 0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0, 
		0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc, 0x0cc, 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0, 
		0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c, 0x15c, 0x055, 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650, 
		0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc, 0x2fc, 0x3f5, 0x0ff, 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0, 
		0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c, 0x36c, 0x265, 0x16f, 0x066, 0x76a, 0x663, 0x569, 0x460, 
		0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac, 0x4ac, 0x5a5, 0x6af, 0x7a6, 0x0aa, 0x1a3, 0x2a9, 0x3a0, 
		0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c, 0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x033, 0x339, 0x230, 
		0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c, 0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x099, 0x190, 
		0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c, 0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x000
	};
	
	//  For each of the possible vertex states listed in cubeEdgeFlags there is a specific triangulation
	//  of the edge intersection points.  triangleConnectionTable lists all of them in the form of
	//  0-5 edge triples with the list terminated by the invalid value -1.
	//  For example: triangleConnectionTable[3] list the 2 triangles formed when corner[0] 
	//  and corner[1] are inside of the surface, but the rest of the cube is not.
	//  triangleConnectionTable[256][16]

	static int[,] triangleConnectionTable = new int[,]  
	{
	    {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1},
	    {3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1},
	    {3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1},
	    {3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1},
	    {9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1},
	    {9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
	    {2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1},
	    {8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1},
	    {9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
	    {4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1},
	    {3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1},
	    {1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1},
	    {4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1},
	    {4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1},
	    {9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1},
	    {1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
	    {5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1},
	    {2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1},
	    {9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
	    {0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
	    {2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1},
	    {10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1},
	    {4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1},
	    {5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1},
	    {5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1},
	    {9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1},
	    {0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1},
	    {1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1},
	    {10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1},
	    {8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1},
	    {2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1},
	    {7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1},
	    {9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1},
	    {2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1},
	    {11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1},
	    {9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1},
	    {5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1},
	    {11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1},
	    {11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
	    {1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1},
	    {9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1},
	    {5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1},
	    {2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
	    {0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
	    {5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1},
	    {6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1},
	    {0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1},
	    {3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1},
	    {6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1},
	    {5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1},
	    {1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
	    {10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1},
	    {6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1},
	    {1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1},
	    {8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1},
	    {7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1},
	    {3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
	    {5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1},
	    {0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1},
	    {9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1},
	    {8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1},
	    {5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1},
	    {0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1},
	    {6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1},
	    {10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1},
	    {10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1},
	    {8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1},
	    {1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1},
	    {3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1},
	    {0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1},
	    {10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1},
	    {0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1},
	    {3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1},
	    {6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1},
	    {9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1},
	    {8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1},
	    {3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1},
	    {6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1},
	    {0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1},
	    {10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1},
	    {10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1},
	    {1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1},
	    {2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1},
	    {7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1},
	    {7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1},
	    {2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1},
	    {1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1},
	    {11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1},
	    {8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1},
	    {0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1},
	    {7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
	    {10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
	    {2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
	    {6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1},
	    {7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1},
	    {2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1},
	    {1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1},
	    {10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1},
	    {10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1},
	    {0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1},
	    {7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1},
	    {6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1},
	    {8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1},
	    {9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1},
	    {6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1},
	    {4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1},
	    {10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1},
	    {8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1},
	    {0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1},
	    {1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1},
	    {8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1},
	    {10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1},
	    {4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1},
	    {10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
	    {5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
	    {11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1},
	    {9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
	    {6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1},
	    {7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1},
	    {3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1},
	    {7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1},
	    {9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1},
	    {3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1},
	    {6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1},
	    {9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1},
	    {1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1},
	    {4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1},
	    {7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1},
	    {6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1},
	    {3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1},
	    {0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1},
	    {6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1},
	    {0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1},
	    {11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1},
	    {6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1},
	    {5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1},
	    {9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1},
	    {1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1},
	    {1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1},
	    {10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1},
	    {0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1},
	    {5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1},
	    {10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1},
	    {11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1},
	    {9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1},
	    {7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1},
	    {2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1},
	    {8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1},
	    {9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1},
	    {9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1},
	    {1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1},
	    {9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1},
	    {9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1},
	    {5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1},
	    {0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1},
	    {10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1},
	    {2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1},
	    {0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1},
	    {0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1},
	    {9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1},
	    {5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1},
	    {3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1},
	    {5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1},
	    {8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1},
	    {9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1},
	    {0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1},
	    {1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1},
	    {3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1},
	    {4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1},
	    {9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1},
	    {11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1},
	    {11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1},
	    {2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1},
	    {9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1},
	    {3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1},
	    {1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1},
	    {4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1},
	    {4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1},
	    {0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1},
	    {3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1},
	    {3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1},
	    {0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1},
	    {9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1},
	    {1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	    {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}
	};
}
