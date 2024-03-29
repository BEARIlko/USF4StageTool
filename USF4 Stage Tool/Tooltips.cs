﻿using System.Collections.Generic;

namespace USF4_Stage_Tool
{
    public static class Tooltips
    {
        public static Dictionary<string, string> tooltips = new Dictionary<string, string>()
        {
            //EMG Context
            ["emgContextInjectOBJ"] = "Inject OBJ by overwriting the selected .EMG",
            ["insertOBJAsNewEMGToolStripMenuItem"] = "Inject OBJ by adding a new .EMG",
            ["deleteEMGToolStripMenuItem"] = "Delete the selected .EMG",
            ["extractEMGAsOBJToolStripMenuItem"] = "Save selected .EMG as an .OBJ file",
            ["extractModelAsOBJToolStripMenuItem"] = "Save selected Mode as an .OBJ file",
            ["extractSubmodelAsOBJToolStripMenuItem"] = "Save selected Submodel as an .OBJ file",
            ["rawDumpEMGToolStripMenuItem"] = "Save selected .EMG as a separate file",
            //EMZ/TEXEMZ Context
            ["saveEMZToolStripMenuItem"] = "Save the selected .EMZ or .TEX.EMZ",
            ["closeEMZToolStripMenuItem"] = "Close the selected .EMZ or .TEX.EMZ",
            ["extractTEXEMZTexturesToolStripMenuItem"] = "Extract all the DDS files from the current .TEX.EMZ. The contents of each .EMB are placed into a separate folder",
            ["AddFileToolStripMenuItem"] = "Add a file of the selected type to the .EMZ or .TEX.EMZ",
            //EMM Context
            ["addNewMaterialToolStripMenuItem"] = "Create a new material with default properties in the current .EMM. If the tool can find an .EMO with a name which matches the .EMM, the .EMO material count is also updated",
            //EMO Context
            ["insertOBJAsNewEMGToolStripMenuItem1"] = "Inject .OBJ by adding a new .EMG to the selected .EMO",
            ["expandAllToolStripMenuItem"] = "Expand all tree nodes",
            ["collapseAllToolStripMenuItem"] = "Collapse all tree nodes",
            ["rawDumpEMOToolStripMenuItem"] = "Save selected .EMO as a separate file",
            ["extractEMOAsOBJToolStripMenuItem"] = "Save selected .EMO as an OBJ file",
            //EMB Context
            ["injectDDSToolStripMenuItem"] = "Add a DDS file to the selected .EMB pack",
            ["addDDSToolStripMenuItem"] = "Delete the selected .EMB pack",
            ["exctractAllDDSToolStripMenuItem"] = "Extract the contents of the .EMB to individual .DDS files",
            //LUA Context
            ["injectLUAScriptToolStripMenuItem1"] = "Takes a plaintext LUA file and attempts to compile it into USF4's native bytecode. This will overwrite the selected LUA file",
            ["extractLUAScriptToolStripMenuItem"] = "Converts the selected LUA bytecode file into plain text and saves it.",
            ["injectLUAScriptToolStripMenuItem"] = "Overwrites the selected LUA file with another bytecode file. Not for use with plaintext LUA files",
            ["extractLUABytecodeToolStripMenuItem"] = "Extract the selected LUA file as bytecode. No conversion to plaintext",
            //DDS Context
            ["addDDSToolStripMenuItem1"] = "Add a DDS file to the current EMB pack",
            ["injectDDSToolStripMenuItem1"] = "Overwrite the selected DDS file",
            ["renameDDSToolStripMenuItem"] = "Rename the selected DDS file. This has no effect in the game engine, but it makes it easier to keep track of textures",
            ["extractDDSToolStripMenuItem"] = "Extract the selected DDS file",
            ["deleteDDSToolStripMenuItem"] = "Delete the selected DDS file",
            //Material Context
            ["addMaterialToolStripMenuItem"] = "Create a new material with default properties in the current EMM. If the tool can find an EMO with a name which matches the EMM, the EMO material count is also updated",
            ["deleteMaterialToolStripMenuItem1"] = "Delete the selected material. May cause the stage to fail to load if the material is still required",
            //CSB Context
            ["injectCSBToolStripMenuItem"] = "Overwrite the selected .CSB file",
            ["extractCSBToolStripMenuItem"] = "Save selected .CSB as a separate file",
            //Animation Context
            ["DeleteAnimaiontoolStripMenuItem3"] = "Delete the selected animation from the .EMA"
        };
    }
}