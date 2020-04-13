// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//-----------------------------------------------------------------------------
//

//
//  Description:
//      Contains method definitions that depend on CD3DDeviceLevel1
//

template <class TVertex>
HRESULT CHwTVertexBuffer<TVertex>::SendVertexFormat(
	__inout_ecount(1) CD3DDeviceLevel1 *pDevice
	) const
{
	RRETURN(THR(pDevice->SetFVF(TVertex::Format)));
}

template <class TVertex>
HRESULT CHwTVertexBuffer<TVertex>::DrawPrimitive(
	__inout_ecount(1) CD3DDeviceLevel1 *pDevice
	) const
{
	HRESULT hr = S_OK;

	Assert(pDevice);

	//
	// Draw the indexed triangle lists.  We might have indexed tri list
	// vertices but not indices if we are aliased and waffling.
	//

	if (m_rgVerticesTriList.GetCount() > 0
		&& m_rgIndices.GetCount() > 0)
	{
		Assert(m_rgIndices.GetCount() % 3 == 0);

		IFC(pDevice->DrawIndexedTriangleListUP(
			m_rgVerticesTriList.GetCount(),
			m_rgIndices.GetCount() / 3, // primitive count
			m_rgIndices.GetDataBuffer(),
			m_rgVerticesTriList.GetDataBuffer(),
			sizeof(TVertex)
			));
	}
	
	//
	// Draw the non-indexed triangle lists
	//

	if (m_rgVerticesNonIndexedTriList.GetCount() > 0)
	{
		Assert(m_rgVerticesNonIndexedTriList.GetCount() %3 == 0);
		
		IFC(pDevice->DrawPrimitiveUP(
			D3DPT_TRIANGLELIST,
			m_rgVerticesNonIndexedTriList.GetCount() / 3, // primitive count
			m_rgVerticesNonIndexedTriList.GetDataBuffer(),
			sizeof(TVertex)
			));
	}

	//
	// Draw the triangle strips
	//

	if (m_rgVerticesTriStrip.GetCount() > 0)
	{
		// A tri strip should have at least 5 vertices including duplicate vertices 
		// at the beginning and end to make degenerate vertices
		Assert(m_rgVerticesTriStrip.GetCount() > 4);

		TVertex *pVertex = static_cast<TVertex *>(m_rgVerticesTriStrip.GetDataBuffer());
		UINT uVertexCount = m_rgVerticesTriStrip.GetCount(); 

		//Check that the tri strip does contain vertces at start and end for the degenerated triangles. 
		Assert(pVertex);
		Assert(pVertex[0].ptPt.Y == pVertex[1].ptPt.Y);
		Assert(pVertex[0].ptPt.X == pVertex[1].ptPt.X); 
		Assert(pVertex[uVertexCount -1].ptPt.Y == pVertex[uVertexCount -2].ptPt.Y);
		Assert(pVertex[uVertexCount -1].ptPt.X == pVertex[uVertexCount -2].ptPt.X); 

		//Remove degenerated triangles from starting and the ending of the vertex buffer. 
		pVertex++;

		IFC(pDevice->DrawPrimitiveUP(
			D3DPT_TRIANGLESTRIP,
			uVertexCount - 4, // primitive count
			pVertex,
			sizeof(TVertex)
			));
	}

	//
	// Draw the line lists
	//

	if (m_rgVerticesLineList.GetCount() > 0)
	{
		IFC(pDevice->DrawPrimitiveUP(
			D3DPT_LINELIST,
			m_rgVerticesLineList.GetCount() / 2, // primitive count
			m_rgVerticesLineList.GetDataBuffer(),
			sizeof(TVertex)
			));
	}

Cleanup:
	RRETURN(hr);
}
