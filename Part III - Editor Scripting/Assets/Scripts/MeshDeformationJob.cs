using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

public struct MeshDeformationJob : IJobParallelFor
{
	public NativeArray<Vector3> initialVerts;
	public NativeArray<Vector3> displacedVerts;
	public NativeArray<Vector3> vertVelocities;

	public float springforce;
	public float dampening;

	public float unitformScale;
	public float time;

	public void Execute(int i)
	{
		Vector3 velocity = vertVelocities[i];
		Vector3 displacement = displacedVerts[i] - 
		initialVerts[i];


		displacement *= unitformScale;
		velocity -= displacement * springforce * time;
		velocity *= unitformScale - dampening * time;

		vertVelocities[i] = velocity;
		displacedVerts[i] += velocity * (time/
		unitformScale);
	}
}
