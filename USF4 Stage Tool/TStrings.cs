using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USF4_Stage_Tool
{
	public static class TStrings
	{
		public static string STR_FlippingGeometryX = "Flipping geometry by X";
		public static string STR_FlippingFacesX = "Flipping faces by X";
		public static string STR_ReadingObject = "Reading OBJ";
		public static string STR_EncodingVerts = "Encoding Verts";
		public static string STR_ReorderingFaces = "Reordering Faces";
		public static string STR_OutputingHex = "Outputing Hex";
		public static string STR_EncodeComplete = "OBJ encoded! You can now over add it as a new EMG, or overwrite an existing EMG";
		public static string STR_OBJSaved = "OBJ Saved!";
		public static string STR_HEXSaved = "HEX Saved!";
		public static string STR_Information = "Information";
		public static string STR_Error = "Error";
		public static string STR_ERR_NoOBJ = "No loaded OBJ file!";
		public static string STR_ERR_VertsNotFoundinOBJ = "Vertices not found in OBJ!";
		public static string STR_ERR_TexturesNotFoundinOBJ = "Textures not found in OBJ!";
		public static string STR_ERR_NormalsNotFoundinOBJ = "Normals not found in OBJ!";
		public static string STR_ERR_FacesNotFoundinOBJ = "Faces not found in OBJ!";
		public static string STR_ERR_FaceDataLonger = "Encoded Face data longer than specified. No padding added!";
	}
}
