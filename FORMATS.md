# Formats
Basic descriptions are provided below, with more in-depth documentation following.  

Format | Extension | Description
------ | --------- | -----------
ACB | .acb | An asset configuration format used in DS2
BHD5 | .bhd, .bhd5 | The header file for the large primary file archives used by most games
BND3 | .\*bnd | A general-purpose file container used before DS2
BND4 | .\*bnd | A general-purpose file container used since DS2
BTAB | .btab | Controls lightmap atlasing
BTL | .btl | Configures point light sources
BTPB | .btpb | Contains baked light probes for a map
BXF3 | .\*bhd + .\*bdt | Equivalent to BND3 but with a separate header and data file
BXF4 | .\*bhd + .\*bdt | Equivalent to BND4 but with a separate header and data file
CCM | .ccm | Determines font layout and texture mapping
CLM2 | .clm | A FLVER companion format that has something to do with cloth
DCX | .dcx | A simple wrapper for a single compressed file
DRB | .drb | Controls GUI layout and styling
EDD | .edd | An ESD companion format that gives friendly names for various elements
EDGE | .edge | Defines grapple points and hangable edges in Sekiro
EMELD | .eld, .emeld | Stores friendly names for EMEVD events
EMEVD | .evd, .emevd | Event scripts
ENFL | .entryfilelist | Specifies assets to preload when going through a load screen
ESD | .esd | Defines a set of state machines used to control characters, menus, dialog, and/or map events
F2TR | .flver2tri | A FLVER companion format that links the vertices to the FaceGen system
FFXDLSE | .ffx | The particle effect format used in DS2
[FLVER](#flver) | .flv, .flver | FromSoftware's standard 3D model format
FMB | .expb | "expression" files introduced in Elden Ring
FMG | .fmg | A collection of strings with corresponding IDs used for most game text
FXR3 | .fxr | The particle effect format used since DS3
GPARAM | .fltparam, .gparam | A generic graphics configuration format
GRASS | .grass | Specifies meshes for grass to be dynamically placed on
LUAGNL | .luagnl | A list of global variable names for Lua scripts
LUAINFO | .luainfo | Information about AI goals for Lua scripts
MCG | .mcg | A high-level navigation format used in DeS and DS1
MCP | .mcp | Another high-level navigation format used in DeS and DS1
[MSB](#msb) | .msb | The main map format, listing all enemies, collisions, trigger volumes, etc
MTD | .mtd | Defines some material and shader properties; referenced by FLVER materials
NGP | .ngp | The navmesh format used in DS2
NVA | .nva | The navmesh format used in BB, DS3, and Sekiro
NVM | .nvm | The navmesh format used in DeS and DS1
[PARAM](#param) | .param | A generic configuration format
PARAMDEF | .def, .paramdef | A companion format that specifies the format of data in a param
PARAMTDF | .tdf | A companion format that provides friendly names for enumerated types in params
PMDCL | .pmdcl | Places and configures static map decals in DS3
RMB | .rmb | Controller rumble effects for all games
TAE3 | .tae | The animation event format used in DS3
TPF | .tpf | A container for platform-specific texture data


<a name="flver"></a>
## FLVER
Because FLVER's format changed significantly between DeS and DS1, two classes are provided: FLVER0, which handles all FLVERs with versions lower than 0x10000, and FLVER2, which handles versions 0x10000 and above. The IFlver interface can used to simplify operating on both types. Common classes are found in the static class FLVER.

<a name="msb"></a>
## MSB
Each game's MSB is supported in a different class: MSB1 for DS1, MSB2 for DS2 (SotFS only), MSB3 for DS3, and MSBS for Sekiro. MSBD and MSBN provide extremely basic support for DeS and Ninja Blade respectively, but cannot be written. Common features are exposed in the currently very limited IMsb interface.

<a name="param"></a>
## PARAM
Params are commonly represented as a spreadsheet, and indeed FromSoft edits them as such. Each row has a unique ID number (sometimes accidentally *not* unique), a friendly name string (stripped out before release in later games), and a block of undifferentiated cell data.  
Because the param itself provides no metadata about how many cells are in each row, what their types are, what they're named, etc, a paramdef must be supplied to parse it into a usable format. Paramdefs were shipped with DeS, DS1, and BB, but other games must have custom ones made.  
A repository of customized paramdefs for most games in the series can be found [here](https://github.com/soulsmods/Paramdex). XML serialization of paramdefs is provided in the library for your convenience along with the original binary format.  
```cs
// Reading an original paramdefbnd
var paramdefs = new List<PARAMDEF>();
var paramdefbnd = BND3.Read(path);
foreach (BinderFile file in paramdefbnd.Files)
{
	var paramdef = PARAMDEF.Read(file.Bytes);
	paramdefs.Add(paramdef);
}
```
```cs
// Reading custom XML paramdefs
var paramdefs = new List<PARAMDEF>();
foreach (string path in Directory.GetFiles(dir, "*.xml"))
{
	var paramdef = PARAMDEF.XmlDeserialize(path);
	paramdefs.Add(paramdef);
}
```
The recommended method of applying paramdefs to params is to call ApplyParamdefCarefully; it will check the param type, data version, and calculated row size to ensure that a paramdef is valid for that param.  
However, if you need to bypass those checks for whatever reason, you may call ApplyParamdef to apply it unconditionally.  
```cs
// Loading a parambnd
var parms = new Dictionary<string, PARAM>();
var parambnd = BND3.Read(path);
foreach (BinderFile file in parambnd.Files)
{
	string name = Path.GetFileNameWithoutExtension(file.Name);
	var param = PARAM.Read(file.Bytes);
	
	// Recommended method: checks the list for any match, or you can test them one-by-one
	if (param.ApplyParamdefCarefully(paramdefs))
		parms[name] = param;
	
	// Alternative method: applies without any additional verification
	param.ApplyParamdef(paramdefs.Find(def => def.ParamType == param.ParamType));
	parms[name] = param;
}

// Editing param data
PARAM weapons = parms["EquipParamWeapon"];
// Paramdef must be supplied to new rows in order to initialize cells correctly
weapons.Rows.Add(new PARAM.Row(9900000, "My Super Cool Weapon", weapons.AppliedParamdef));
// Be mindful of the correct value type for each cell
weapons[9900000]["weight"].Value = 100f;

// Save each param, then the parambnd
foreach (BinderFile file in parambnd.Files)
{
	string name = Path.GetFileNameWithoutExtension(file.Name);
	if (parms.ContainsKey(name))
		file.Bytes = parms[name].Write();
}
parambnd.Write(path);
```
