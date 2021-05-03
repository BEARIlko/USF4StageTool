using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USF4_Stage_Tool
{
    class CodeStrings
    {
        //This class contains a reproduction of the majority of the code of ChunkSpy.lua.
        //As such, its license is reproduced below.

        //ChunkSpy.lua License
        //--------------------

        //ChunkSpy.lua is licensed under the terms of the MIT license reproduced
        //below.This means that ChunkSpy.lua is free software and can be used for
        //both academic and commercial purposes at absolutely no cost.

        //For details and rationale, see http://www.lua.org/license.html .

        //===============================================================================

        //Copyright (C) 2004-2006 Kein-Hong Man <khman@users.sf.net>

        //Permission is hereby granted, free of charge, to any person obtaining a copy
        //of this software and associated documentation files (the "Software"), to deal
        //in the Software without restriction, including without limitation the rights
        //to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        //copies of the Software, and to permit persons to whom the Software is
        //furnished to do so, subject to the following conditions:

        //The above copyright notice and this permission notice shall be included in
        //all copies or substantial portions of the Software.

        //THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        //IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        //FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
        //AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        //LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        //OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        //THE SOFTWARE.

        //===============================================================================

        //(end of COPYRIGHT)

        public static string infile = "native_lua_chunk.out";
        public static string outfile = "output_usf4.out";

        public static string ChunkSpy1 = "title = [[]]\r\n" +
           "USAGE = [[]]\r\n" +
           "interactive_help = [" +
           "[]]\r\n" +
           "CONFIGURATION = {\r" +
           "\n" +
           "  [\"x86 standard\"]" +
           " = {\r\n" +
           "    description = \"" +
           "x86\",\r\n" +
           "    endianness = 1," +
           "\r\n" +
           "    size_int = 4,\r" +
           "\n" +
           "    size_size_t = 4," +
           "\r\n" +
           "    size_Instruction" +
           " = 4,\r\n" +
           "    size_lua_Number " +
           "= 8,\r\n" +
           "    integral = 0,\r" +
           "\n" +
           "    number_type = \"" +
           "double\",\r\n" +
           "  },\r\n" +
           "  [\"big endian int" +
           "\"] = {\r\n" +
           "    description = \"" +
           "\",\r\n" +
           "    endianness = 0," +
           "\r\n" +
           "    size_int = 4,\r" +
           "\n" +
           "    size_size_t = 4," +
           "\r\n" +
           "    size_Instruction" +
           " = 4,\r\n" +
           "    size_lua_Number " +
           "= 4,\r\n" +
           "    integral = 1,\r" +
           "\n" +
           "    number_type = \"" +
           "int\",\r\n" +
           "  },\r\n" +
           "  [\"USF4\"] = {\r\n" +
           "\tdescription = \"US" +
           "F4\",\r\n" +
           "\tendianness = 1,\r" +
           "\n" +
           "\tsize_int = 4,\r\n" +
           "\tsize_size_t = 4,\r" +
           "\n" +
           "\tsize_Instruction =" +
           " 4,\r\n" +
           "\tsize_lua_Number = " +
           "4,\r\n" +
           "\tintegral = 0,\r\n" +
           "\tnumber_type = \"si" +
           "ngle\",\r\n" +
           "  },\r\n" +
           "}\r\n" +
           "config = {}\r\n" +
           "function SetProfile(" +
           "profile)\r\n" +
           "    local c = CONFIG" +
           "URATION[profile]\r\n" +
           "    if not c then re" +
           "turn false end\r\n" +
           "    for i, v in pair" +
           "s(c) do config[i] = " +
           "v end\r\n" +
           "  return true\r\n" +
           "end\r\n" +
           "SetProfile(\"USF4\")" +
           "\r\n" +
           "config.SIGNATURE    " +
           "= \"\\27Lua\"\r\n" +
           "config.LUA_TNIL     " +
           "= 0\r\n" +
           "config.LUA_TBOOLEAN " +
           "= 1\r\n" +
           "config.LUA_TNUMBER  " +
           "= 3\r\n" +
           "config.LUA_TSTRING  " +
           "= 4\r\n" +
           "config.VERSION      " +
           "= 81\r\n" +
           "config.FORMAT       " +
           "= 0\r\n" +
           "config.FPF          " +
           "= 50\r\n" +
           "config.SIZE_OP      " +
           "= 6\r\n" +
           "config.SIZE_A       " +
           "= 8\r\n" +
           "config.SIZE_B       " +
           "= 9\r\n" +
           "config.SIZE_C       " +
           "= 9\r\n" +
           "config.LUA_FIRSTINDE" +
           "X = 1\r\n" +
           "config.DISPLAY_FLAG " +
           "= true\r\n" +
           "config.DISPLAY_BRIEF" +
           " = nil\r\n" +
           "config.DISPLAY_INDEN" +
           "T = nil\r\n" +
           "config.STATS = nil\r" +
           "\n" +
           "config.DISPLAY_OFFSE" +
           "T_HEX = true\r\n" +
           "config.DISPLAY_SEP =" +
           " \"  \"\r\n" +
           "config.DISPLAY_COMME" +
           "NT = \"; \"\r\n" +
           "config.DISPLAY_HEX_D" +
           "ATA = true\r\n" +
           "config.WIDTH_HEX = 8" +
           "\r\n" +
           "config.WIDTH_OFFSET " +
           "= nil\r\n" +
           "config.DISPLAY_LOWER" +
           "CASE = true\r\n" +
           "config.WIDTH_OPCODE " +
           "= nil\r\n" +
           "config.VERBOSE_TEST " +
           "= false\r\n" +
           "other_files = {}\r\n" +
           "arg_other = {}\r\n" +
           "convert_from = {}\r" +
           "\n" +
           "convert_to = {}\r\n" +
           "function grab_byte(v" +
           ")\r\n" +
           "  return math.floor(" +
           "v / 256), string.cha" +
           "r(math.floor(v) % 25" +
           "6)\r\n" +
           "end\r\n" +
           "LUANUMBER_ID = {\r\n" +
           "  [\"80\"] = \"doubl" +
           "e\",\r\n" +
           "  [\"40\"] = \"singl" +
           "e\",\r\n" +
           "  [\"41\"] = \"int\"" +
           ",\r\n" +
           "  [\"81\"] = \"long " +
           "long\",\r\n" +
           "}\r\n" +
           "convert_from[\"doubl" +
           "e\"] = function(x)\r" +
           "\n" +
           "  local sign = 1\r\n" +
           "  local mantissa = s" +
           "tring.byte(x, 7) % 1" +
           "6\r\n" +
           "  for i = 6, 1, -1 d" +
           "o mantissa = mantiss" +
           "a * 256 + string.byt" +
           "e(x, i) end\r\n" +
           "  if string.byte(x, " +
           "8) > 127 then sign =" +
           " -1 end\r\n" +
           "  local exponent = (" +
           "string.byte(x, 8) % " +
           "128) * 16 +\r\n" +
           "                   m" +
           "ath.floor(string.byt" +
           "e(x, 7) / 16)\r\n" +
           "  if exponent == 0 t" +
           "hen return 0 end\r\n" +
           "  mantissa = (math.l" +
           "dexp(mantissa, -52) " +
           "+ 1) * sign\r\n" +
           "  return math.ldexp(" +
           "mantissa, exponent -" +
           " 1023)\r\n" +
           "end\r\n" +
           "convert_from[\"singl" +
           "e\"] = function(x)\r" +
           "\n" +
           "  local sign = 1\r\n" +
           "  local mantissa = s" +
           "tring.byte(x, 3) % 1" +
           "28\r\n" +
           "  for i = 2, 1, -1 d" +
           "o mantissa = mantiss" +
           "a * 256 + string.byt" +
           "e(x, i) end\r\n" +
           "  if string.byte(x, " +
           "4) > 127 then sign =" +
           " -1 end\r\n" +
           "  local exponent = (" +
           "string.byte(x, 4) % " +
           "128) * 2 +\r\n" +
           "                   m" +
           "ath.floor(string.byt" +
           "e(x, 3) / 128)\r\n" +
           "  if exponent == 0 t" +
           "hen return 0 end\r\n" +
           "  mantissa = (math.l" +
           "dexp(mantissa, -23) " +
           "+ 1) * sign\r\n" +
           "  return math.ldexp(" +
           "mantissa, exponent -" +
           " 127)\r\n" +
           "end\r\n" +
           "convert_from[\"int\"" +
           "] = function(x)\r\n" +
           "  local sum = 0\r\n" +
           "  for i = config.siz" +
           "e_lua_Number, 1, -1 " +
           "do\r\n" +
           "    sum = sum * 256 " +
           "+ string.byte(x, i)" +
           "\r\n" +
           "  end\r\n" +
           "  if string.byte(x, " +
           "config.size_lua_Numb" +
           "er) > 127 then\r\n" +
           "    sum = sum - math" +
           ".ldexp(1, 8 * config" +
           ".size_lua_Number)\r" +
           "\n" +
           "  end\r\n" +
           "  return sum\r\n" +
           "end\r\n" +
           "convert_from[\"long " +
           "long\"] = convert_fr" +
           "om[\"int\"]\r\n" +
           "convert_to[\"double" +
           "\"] = function(x)\r" +
           "\n" +
           "  local sign = 0\r\n" +
           "  if x < 0 then sign" +
           " = 1; x = -x end\r\n" +
           "  local mantissa, ex" +
           "ponent = math.frexp(" +
           "x)\r\n" +
           "  if x == 0 then\r\n" +
           "    mantissa, expone" +
           "nt = 0, 0\r\n" +
           "  else\r\n" +
           "    mantissa = (mant" +
           "issa * 2 - 1) * math" +
           ".ldexp(0.5, 53)\r\n" +
           "    exponent = expon" +
           "ent + 1022\r\n" +
           "  end\r\n" +
           "  local v, byte = \"" +
           "\"\r\n" +
           "  x = mantissa\r\n" +
           "  for i = 1,6 do\r\n" +
           "    x, byte = grab_b" +
           "yte(x); v = v..byte" +
           "\r\n" +
           "  end\r\n" +
           "  x, byte = grab_byt" +
           "e(exponent * 16 + x)" +
           "; v = v..byte\r\n" +
           "  x, byte = grab_byt" +
           "e(sign * 128 + x); v" +
           " = v..byte\r\n" +
           "  return v\r\n" +
           "end\r\n" +
           "convert_to[\"single" +
           "\"] = function(x)\r" +
           "\n" +
           "  local sign = 0\r\n" +
           "  if x < 0 then sign" +
           " = 1; x = -x end\r\n" +
           "  local mantissa, ex" +
           "ponent = math.frexp(" +
           "x)\r\n" +
           "  if x == 0 then\r\n" +
           "    mantissa = 0; ex" +
           "ponent = 0\r\n" +
           "  else\r\n" +
           "    mantissa = (mant" +
           "issa * 2 - 1) * math" +
           ".ldexp(0.5, 24)\r\n" +
           "    exponent = expon" +
           "ent + 126\r\n" +
           "  end\r\n" +
           "  local v, byte = \"" +
           "\"\r\n" +
           "  x, byte = grab_byt" +
           "e(mantissa); v = v.." +
           "byte\r\n" +
           "  x, byte = grab_byt" +
           "e(x); v = v..byte\r" +
           "\n" +
           "  x, byte = grab_byt" +
           "e(exponent * 128 + x" +
           "); v = v..byte\r\n" +
           "  x, byte = grab_byt" +
           "e(sign * 128 + x); v" +
           " = v..byte\r\n" +
           "  return v\r\n" +
           "end\r\n" +
           "convert_to[\"int\"] " +
           "= function(x)\r\n" +
           "  local v = \"\"\r\n" +
           "  x = math.floor(x)" +
           "\r\n" +
           "  if x >= 0 then\r\n" +
           "    for i = 1, confi" +
           "g.size_lua_Number do" +
           "\r\n" +
           "      v = v..string." +
           "char(x % 256); x = m" +
           "ath.floor(x / 256)\r" +
           "\n" +
           "    end\r\n" +
           "  else\r\n" +
           "    x = -x\r\n" +
           "    local carry = 1" +
           "\r\n" +
           "    for i = 1, confi" +
           "g.size_lua_Number do" +
           "\r\n" +
           "      local c = 255 " +
           "- (x % 256) + carry" +
           "\r\n" +
           "      if c == 256 th" +
           "en c = 0; carry = 1 " +
           "else carry = 0 end\r" +
           "\n" +
           "      v = v..string." +
           "char(c); x = math.fl" +
           "oor(x / 256)\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  return v\r\n" +
           "end\r\n" +
           "convert_to[\"long lo" +
           "ng\"] = convert_to[" +
           "\"int\"]\r\n" +
           "function WidthOf(n) " +
           "return string.len(to" +
           "string(n)) end\r\n" +
           "function LeftJustify" +
           "(s, width) return s." +
           ".string.rep(\" \", w" +
           "idth - string.len(s)" +
           ") end\r\n" +
           "function ZeroPad(s, " +
           "width) return string" +
           ".rep(\"0\", width - " +
           "string.len(s))..s en" +
           "d\r\n" +
           "function DisplayInit" +
           "(chunk_size)\r\n" +
           "  if not config.WIDT" +
           "H_OFFSET then config" +
           ".WIDTH_OFFSET = 0 en" +
           "d\r\n" +
           "  if config.DISPLAY_" +
           "OFFSET_HEX then\r\n" +
           "    local w = string" +
           ".len(string.format(" +
           "\"%X\", chunk_size))" +
           "\r\n" +
           "    if w > config.WI" +
           "DTH_OFFSET then conf" +
           "ig.WIDTH_OFFSET = w " +
           "end\r\n" +
           "    if (config.WIDTH" +
           "_OFFSET % 2) == 1 th" +
           "en\r\n" +
           "      config.WIDTH_O" +
           "FFSET = config.WIDTH" +
           "_OFFSET + 1\r\n" +
           "    end\r\n" +
           "  else\r\n" +
           "    config.WIDTH_OFF" +
           "SET = string.len(ton" +
           "umber(chunk_size))\r" +
           "\n" +
           "  end\r\n" +
           "  if config.WIDTH_OF" +
           "FSET < 4 then config" +
           ".WIDTH_OFFSET = 4 en" +
           "d\r\n" +
           "  if not config.DISP" +
           "LAY_SEP then config." +
           "DISPLAY_SEP = \"  \"" +
           " end\r\n" +
           "  if config.DISPLAY_" +
           "HEX_DATA == nil then" +
           " config.DISPLAY_HEX_" +
           "DATA = true end\r\n" +
           "  if not config.WIDT" +
           "H_HEX then config.WI" +
           "DTH_HEX = 8 end\r\n" +
           "  config.BLANKS_HEX_" +
           "DATA = string.rep(\"" +
           " \", config.WIDTH_HE" +
           "X * 2 + 1)\r\n" +
           "  if not WriteLine t" +
           "hen WriteLine = prin" +
           "t end\r\n" +
           "end\r\n" +
           "function OutputInit(" +
           ")\r\n" +
           "  if config.OUTPUT_F" +
           "ILE then\r\n" +
           "    if type(config.O" +
           "UTPUT_FILE) == \"str" +
           "ing\" then\r\n" +
           "      local INF = io" +
           ".open(config.OUTPUT_" +
           "FILE, \"wb\")\r\n" +
           "      if not INF the" +
           "n\r\n" +
           "        error(\"cann" +
           "ot open \\\"\"..conf" +
           "ig.OUTPUT_FILE..\"\\" +
           "\" for writing\")\r" +
           "\n" +
           "      end\r\n" +
           "      config.OUTPUT_" +
           "FILE = INF\r\n" +
           "      WriteLine = fu" +
           "nction(s) config.OUT" +
           "PUT_FILE:write(s, \"" +
           "\\n\") end\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "end\r\n" +
           "function OutputExit(" +
           ")\r\n" +
           "  if WriteLine and W" +
           "riteLine ~= print th" +
           "en io.close(config.O" +
           "UTPUT_FILE) end\r\n" +
           "end\r\n" +
           "function EscapeStrin" +
           "g(s, quoted)\r\n" +
           "  local v = \"\"\r\n" +
           "  for i = 1, string." +
           "len(s) do\r\n" +
           "    local c = string" +
           ".byte(s, i)\r\n" +
           "    if c < 32 or c =" +
           "= 34 or c == 92 then" +
           "\r\n" +
           "      if c >= 7 and " +
           "c <= 13 then\r\n" +
           "        c = string.s" +
           "ub(\"abtnvfr\", c - " +
           "6, c - 6)\r\n" +
           "      elseif c == 34" +
           " or c == 92 then\r\n" +
           "        c = string.c" +
           "har(c)\r\n" +
           "      end\r\n" +
           "      v = v..\"\\\\" +
           "\"..c\r\n" +
           "    else\r\n" +
           "      v = v..string." +
           "char(c)\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  if quoted then ret" +
           "urn string.format(\"" +
           "\\\"%s\\\"\", v) end" +
           "\r\n" +
           "  return v\r\n" +
           "end\r\n" +
           "function HeaderLine(" +
           ")\r\n" +
           "  if not config.DISP" +
           "LAY_FLAG or config.D" +
           "ISPLAY_BRIEF then re" +
           "turn end\r\n" +
           "  WriteLine(LeftJust" +
           "ify(\"Pos\", config." +
           "WIDTH_OFFSET)..confi" +
           "g.DISPLAY_SEP\r\n" +
           "            ..LeftJu" +
           "stify(\"Hex Data\", " +
           "config.WIDTH_HEX * 2" +
           " + 1)..config.DISPLA" +
           "Y_SEP\r\n" +
           "            ..\"Desc" +
           "ription or Code\\n\"" +
           "\r\n" +
           "            ..string" +
           ".rep(\"-\", 72))\r\n" +
           "end\r\n" +
           "function DescLine(de" +
           "sc)\r\n" +
           "  if not config.DISP" +
           "LAY_FLAG or config.D" +
           "ISPLAY_BRIEF then re" +
           "turn end\r\n" +
           "  WriteLine(string.r" +
           "ep(\" \", config.WID" +
           "TH_OFFSET)..config.D" +
           "ISPLAY_SEP\r\n" +
           "            ..config" +
           ".BLANKS_HEX_DATA..co" +
           "nfig.DISPLAY_SEP\r\n" +
           "            ..desc)" +
           "\r\n" +
           "end\r\n" +
           "function DisplayStat" +
           "(stat)\r\n" +
           "  if config.STATS an" +
           "d not config.DISPLAY" +
           "_BRIEF then DescLine" +
           "(stat) end\r\n" +
           "end\r\n" +
           "function FormatPos(i" +
           ")\r\n" +
           "  local pos\r\n" +
           "  if config.DISPLAY_" +
           "OFFSET_HEX then\r\n" +
           "    pos = string.for" +
           "mat(\"%X\", i - 1)\r" +
           "\n" +
           "  else\r\n" +
           "    pos = tonumber(i" +
           " - 1)\r\n" +
           "  end\r\n" +
           "  return ZeroPad(pos" +
           ", config.WIDTH_OFFSE" +
           "T)\r\n" +
           "end\r\n" +
           "function DecodeInit(" +
           ")\r\n" +
           "  config.SIZE_Bx = c" +
           "onfig.SIZE_B + confi" +
           "g.SIZE_C\r\n" +
           "  local MASK_OP = ma" +
           "th.ldexp(1, config.S" +
           "IZE_OP)\r\n" +
           "  local MASK_A  = ma" +
           "th.ldexp(1, config.S" +
           "IZE_A)\r\n" +
           "  local MASK_B  = ma" +
           "th.ldexp(1, config.S" +
           "IZE_B)\r\n" +
           "  local MASK_C  = ma" +
           "th.ldexp(1, config.S" +
           "IZE_C)\r\n" +
           "  local MASK_Bx = ma" +
           "th.ldexp(1, config.S" +
           "IZE_Bx)\r\n" +
           "  config.MAXARG_sBx " +
           "= math.floor((MASK_B" +
           "x - 1) / 2)\r\n" +
           "  config.BITRK = mat" +
           "h.ldexp(1, config.SI" +
           "ZE_B - 1)\r\n" +
           "  config.iABC = {\r" +
           "\n" +
           "    config.SIZE_OP," +
           "\r\n" +
           "    config.SIZE_A,\r" +
           "\n" +
           "    config.SIZE_C,\r" +
           "\n" +
           "    config.SIZE_B,\r" +
           "\n" +
           "  }\r\n" +
           "  config.mABC = { MA" +
           "SK_OP, MASK_A, MASK_" +
           "C, MASK_B, }\r\n" +
           "  config.nABC = { \"" +
           "OP\", \"A\", \"C\", " +
           "\"B\", }\r\n" +
           "  local op =\r\n" +
           "    \"MOVE LOADK LOA" +
           "DBOOL LOADNIL GETUPV" +
           "AL \\\r\n" +
           "    GETGLOBAL GETTAB" +
           "LE SETGLOBAL SETUPVA" +
           "L SETTABLE \\\r\n" +
           "    NEWTABLE SELF AD" +
           "D SUB MUL \\\r\n" +
           "    DIV MOD POW UNM " +
           "NOT \\\r\n" +
           "    LEN CONCAT JMP E" +
           "Q LT \\\r\n" +
           "    LE TEST TESTSET " +
           "CALL TAILCALL RETURN" +
           " \\\r\n" +
           "    FORLOOP FORPREP " +
           "TFORLOOP SETLIST \\" +
           "\r\n" +
           "    CLOSE CLOSURE VA" +
           "RARG\"\r\n" +
           "  config.opnames = {" +
           "}\r\n" +
           "  config.NUM_OPCODES" +
           " = 0\r\n" +
           "  if not config.WIDT" +
           "H_OPCODE then config" +
           ".WIDTH_OPCODE = 0 en" +
           "d\r\n" +
           "  for v in string.gm" +
           "atch(op, \"[^%s]+\")" +
           " do\r\n" +
           "    if config.DISPLA" +
           "Y_LOWERCASE then v =" +
           " string.lower(v) end" +
           "\r\n" +
           "    config.opnames[c" +
           "onfig.NUM_OPCODES] =" +
           " v\r\n" +
           "    local vlen = str" +
           "ing.len(v)\r\n" +
           "    if vlen > config" +
           ".WIDTH_OPCODE then\r" +
           "\n" +
           "      config.WIDTH_O" +
           "PCODE = vlen\r\n" +
           "    end\r\n" +
           "    config.NUM_OPCOD" +
           "ES = config.NUM_OPCO" +
           "DES + 1\r\n" +
           "  end\r\n" +
           "  config.opmode = \"" +
           "01000101000000000000" +
           "002000000002200010\"" +
           "\r\n" +
           "  config.WIDTH_A = W" +
           "idthOf(MASK_A)\r\n" +
           "  config.WIDTH_B = W" +
           "idthOf(MASK_B)\r\n" +
           "  config.WIDTH_C = W" +
           "idthOf(MASK_C)\r\n" +
           "  config.WIDTH_Bx = " +
           "WidthOf(MASK_Bx) + 1" +
           "\r\n" +
           "  config.FORMAT_A = " +
           "string.format(\"%%-%" +
           "dd\", config.WIDTH_A" +
           ")\r\n" +
           "  config.FORMAT_B = " +
           "string.format(\"%%-%" +
           "dd\", config.WIDTH_B" +
           ")\r\n" +
           "  config.FORMAT_C = " +
           "string.format(\"%%-%" +
           "dd\", config.WIDTH_C" +
           ")\r\n" +
           "  config.PAD_Bx = co" +
           "nfig.WIDTH_A + confi" +
           "g.WIDTH_B + config.W" +
           "IDTH_C + 2\r\n" +
           "                  - " +
           "config.WIDTH_Bx\r\n" +
           "  if config.PAD_Bx >" +
           " 0 then\r\n" +
           "    config.PAD_Bx = " +
           "string.rep(\" \", co" +
           "nfig.PAD_Bx)\r\n" +
           "  else\r\n" +
           "    config.PAD_Bx = " +
           "\"\"\r\n" +
           "  end\r\n" +
           "  config.FORMAT_Bx  " +
           "= string.format(\"%%" +
           "-%dd\", config.WIDTH" +
           "_Bx)\r\n" +
           "  config.FORMAT_AB  " +
           "= string.format(\"%s" +
           " %s %s\", config.FOR" +
           "MAT_A, config.FORMAT" +
           "_B, string.rep(\" \"" +
           ", config.WIDTH_C))\r" +
           "\n" +
           "  config.FORMAT_ABC " +
           "= string.format(\"%s" +
           " %s %s\", config.FOR" +
           "MAT_A, config.FORMAT" +
           "_B, config.FORMAT_C)" +
           "\r\n" +
           "  config.FORMAT_AC  " +
           "= string.format(\"%s" +
           " %s %s\", config.FOR" +
           "MAT_A, string.rep(\"" +
           " \", config.WIDTH_B)" +
           ", config.FORMAT_C)\r" +
           "\n" +
           "  config.FORMAT_ABx " +
           "= string.format(\"%s" +
           " %s\", config.FORMAT" +
           "_A, config.FORMAT_Bx" +
           ")\r\n" +
           "end\r\n" +
           "function DecodeInst(" +
           "code, iValues)\r\n" +
           "  local iSeq, iMask " +
           "= config.iABC, confi" +
           "g.mABC\r\n" +
           "  local cValue, cBit" +
           "s, cPos = 0, 0, 1\r" +
           "\n" +
           "  for i = 1, #iSeq d" +
           "o\r\n" +
           "    while cBits < iS" +
           "eq[i] do\r\n" +
           "      cValue = strin" +
           "g.byte(code, cPos) *" +
           " math.ldexp(1, cBits" +
           ") + cValue\r\n" +
           "      cPos = cPos + " +
           "1; cBits = cBits + 8" +
           "\r\n" +
           "    end\r\n" +
           "    iValues[config.n" +
           "ABC[i]] = cValue % i" +
           "Mask[i]\r\n" +
           "    cValue = math.fl" +
           "oor(cValue / iMask[i" +
           "])\r\n" +
           "    cBits = cBits - " +
           "iSeq[i]\r\n" +
           "  end\r\n" +
           "  iValues.opname = c" +
           "onfig.opnames[iValue" +
           "s.OP]\r\n" +
           "  iValues.opmode = s" +
           "tring.sub(config.opm" +
           "ode, iValues.OP + 1," +
           " iValues.OP + 1)\r\n" +
           "  if iValues.opmode " +
           "== \"1\" then\r\n" +
           "    iValues.Bx = iVa" +
           "lues.B * iMask[3] + " +
           "iValues.C\r\n" +
           "  elseif iValues.opm" +
           "ode == \"2\" then\r" +
           "\n" +
           "    iValues.sBx = iV" +
           "alues.B * iMask[3] +" +
           " iValues.C - config." +
           "MAXARG_sBx\r\n" +
           "  end\r\n" +
           "  return iValues\r\n" +
           "end\r\n" +
           "function EncodeInst(" +
           "inst)\r\n" +
           "  local v, i = \"\"," +
           " 0\r\n" +
           "  local cValue, cBit" +
           "s, cPos = 0, 0, 1\r" +
           "\n" +
           "  while i < config.s" +
           "ize_Instruction do\r" +
           "\n" +
           "    while cBits < 8 " +
           "do\r\n" +
           "      cValue = inst[" +
           "config.nABC[cPos]] *" +
           " math.ldexp(1, cBits" +
           ") + cValue\r\n" +
           "      cBits = cBits " +
           "+ config.iABC[cPos];" +
           " cPos = cPos + 1\r\n" +
           "    end\r\n" +
           "    while cBits >= 8" +
           " do\r\n" +
           "      v = v..string." +
           "char(cValue % 256)\r" +
           "\n" +
           "      cValue = math." +
           "floor(cValue / 256)" +
           "\r\n" +
           "      cBits = cBits " +
           "- 8; i = i + 1\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  return v\r\n" +
           "end\r\n" +
           "function DescribeIns" +
           "t(inst, pos, func)\r" +
           "\n" +
           "  local Operand\r\n" +
           "  local Comment = \"" +
           "\"\r\n" +
           "  local function Ope" +
           "randAB(i)   return s" +
           "tring.format(config." +
           "FORMAT_AB, i.A, i.B)" +
           " end\r\n" +
           "  local function Ope" +
           "randABC(i)  return s" +
           "tring.format(config." +
           "FORMAT_ABC, i.A, i.B" +
           ", i.C) end\r\n" +
           "  local function Ope" +
           "randAC(i)   return s" +
           "tring.format(config." +
           "FORMAT_AC, i.A, i.C)" +
           " end\r\n" +
           "  local function Ope" +
           "randABx(i)  return s" +
           "tring.format(config." +
           "FORMAT_ABx, i.A, i.B" +
           "x) end\r\n" +
           "  local function Ope" +
           "randAsBx(i) return s" +
           "tring.format(config." +
           "FORMAT_ABx, i.A, i.s" +
           "Bx) end\r\n" +
           "  local function Com" +
           "mentLoc(sBx, cond)\r" +
           "\n" +
           "    local loc = stri" +
           "ng.format(\"to [%d]" +
           "\", pos + 1 + sBx)\r" +
           "\n" +
           "    if cond then loc" +
           " = loc..cond end\r\n" +
           "    return loc\r\n" +
           "  end\r\n" +
           "  local function Com" +
           "mentK(index, quoted)" +
           "\r\n" +
           "    local c = func.k" +
           "[index + 1]\r\n" +
           "    if type(c) == \"" +
           "string\" then\r\n" +
           "      return EscapeS" +
           "tring(c, quoted)\r\n" +
           "    elseif type(c) =" +
           "= \"number\" or type" +
           "(c) == \"boolean\" t" +
           "hen\r\n" +
           "      return tostrin" +
           "g(c)\r\n" +
           "    else\r\n" +
           "      return \"nil\"" +
           "\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  local function Com" +
           "mentRK(index, quoted" +
           ")\r\n" +
           "    if index >= conf" +
           "ig.BITRK then\r\n" +
           "      return Comment" +
           "K(index - config.BIT" +
           "RK, quoted)\r\n" +
           "    else\r\n" +
           "      return \"\"\r" +
           "\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  local function Com" +
           "mentBC(inst)\r\n" +
           "    local B, C = Com" +
           "mentRK(inst.B, true)" +
           ", CommentRK(inst.C, " +
           "true)\r\n" +
           "    if B == \"\" the" +
           "n\r\n" +
           "      if C == \"\" t" +
           "hen return \"\" else" +
           " return C end\r\n" +
           "    elseif C == \"\"" +
           " then\r\n" +
           "      return B\r\n" +
           "    else\r\n" +
           "      return B..\" " +
           "\"..C\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  local function fb2" +
           "int(x)\r\n" +
           "    local e = math.f" +
           "loor(x / 8) % 32\r\n" +
           "    if e == 0 then r" +
           "eturn x end\r\n" +
           "    return math.ldex" +
           "p((x % 8) + 8, e - 1" +
           ")\r\n" +
           "  end\r\n" +
           "  if inst.prev then" +
           "\r\n" +
           "    Operand = string" +
           ".format(config.FORMA" +
           "T_Bx, func.code[pos]" +
           ")..config.PAD_Bx\r\n" +
           "  elseif inst.OP == " +
           " 0 then\r\n" +
           "    Operand = Operan" +
           "dAB(inst)\r\n" +
           "  elseif inst.OP == " +
           " 1 then\r\n" +
           "    Operand = Operan" +
           "dABx(inst)\r\n" +
           "    Comment = Commen" +
           "tK(inst.Bx, true)\r" +
           "\n" +
           "  elseif inst.OP == " +
           " 2 then\r\n" +
           "    Operand = Operan" +
           "dABC(inst)\r\n" +
           "    if inst.B == 0 t" +
           "hen Comment = \"fals" +
           "e\" else Comment = " +
           "\"true\" end\r\n" +
           "    if inst.C > 0 th" +
           "en Comment = Comment" +
           "..\", \"..CommentLoc" +
           "(1) end\r\n" +
           "  elseif inst.OP == " +
           " 3 then\r\n" +
           "    Operand = Operan" +
           "dAB(inst)\r\n" +
           "  elseif inst.OP == " +
           " 4 then\r\n" +
           "    Operand = Operan" +
           "dAB(inst)\r\n" +
           "    Comment = func.u" +
           "pvalues[inst.B + 1]" +
           "\r\n" +
           "  elseif inst.OP == " +
           " 5 or\r\n" +
           "         inst.OP == " +
           " 7 then\r\n" +
           "    Operand = Operan" +
           "dABx(inst)\r\n" +
           "    Comment = Commen" +
           "tK(inst.Bx)\r\n" +
           "  elseif inst.OP == " +
           " 6 then\r\n" +
           "    Operand = Operan" +
           "dABC(inst)\r\n" +
           "    Comment = Commen" +
           "tRK(inst.C, true)\r" +
           "\n" +
           "  elseif inst.OP == " +
           " 8 then\r\n" +
           "    Operand = Operan" +
           "dAB(inst)\r\n" +
           "    Comment = func.u" +
           "pvalues[inst.B + 1]" +
           "\r\n" +
           "  elseif inst.OP == " +
           " 9 then\r\n" +
           "    Operand = Operan" +
           "dABC(inst)\r\n" +
           "    Comment = Commen" +
           "tBC(inst)\r\n" +
           "  elseif inst.OP == " +
           "10 then\r\n" +
           "    Operand = Operan" +
           "dABC(inst)\r\n" +
           "    local ar = fb2in" +
           "t(inst.B) \r\n" +
           "    local hs = fb2in" +
           "t(inst.C)\r\n" +
           "    Comment = \"arra" +
           "y=\"..ar..\", hash=" +
           "\"..hs\r\n" +
           "  elseif inst.OP == " +
           "11 then\r\n" +
           "    Operand = Operan" +
           "dABC(inst)\r\n" +
           "    Comment = Commen" +
           "tRK(inst.C, true)\r" +
           "\n" +
           "  elseif inst.OP == " +
           "12 or\r\n" +
           "         inst.OP == " +
           "13 or\r\n" +
           "         inst.OP == " +
           "14 or \r\n" +
           "         inst.OP == " +
           "15 or\r\n" +
           "         inst.OP == " +
           "16 or \r\n" +
           "         inst.OP == " +
           "17 then\r\n" +
           "    Operand = Operan" +
           "dABC(inst)\r\n" +
           "    Comment = Commen" +
           "tBC(inst)\r\n" +
           "  elseif inst.OP == " +
           "18 or\r\n" +
           "         inst.OP == " +
           "19 or\r\n" +
           "         inst.OP == " +
           "20 then\r\n" +
           "    Operand = Operan" +
           "dAB(inst)\r\n" +
           "  elseif inst.OP == " +
           "21 then\r\n" +
           "    Operand = Operan" +
           "dABC(inst)\r\n" +
           "  elseif inst.OP == " +
           "22 then\r\n" +
           "    Operand = string" +
           ".format(config.FORMA" +
           "T_Bx, inst.sBx)..con" +
           "fig.PAD_Bx\r\n" +
           "    Comment = Commen" +
           "tLoc(inst.sBx)\r\n" +
           "  elseif inst.OP == " +
           "23 or\r\n" +
           "         inst.OP == " +
           "24 or\r\n" +
           "         inst.OP == " +
           "25 or\r\n" +
           "         inst.OP == " +
           "27 then\r\n" +
           "    Operand = Operan" +
           "dABC(inst)\r\n" +
           "    if inst.OP ~= 27" +
           " then Comment = Comm" +
           "entBC(inst) end\r\n" +
           "    if Comment ~= \"" +
           "\" then Comment = Co" +
           "mment..\", \" end\r" +
           "\n" +
           "    local sense = \"" +
           " if false\"\r\n" +
           "    if inst.OP == 27" +
           " then\r\n" +
           "      if inst.C == 0" +
           " then sense = \" if " +
           "true\" end\r\n" +
           "    else\r\n" +
           "      if inst.A == 0" +
           " then sense = \" if " +
           "true\" end\r\n" +
           "    end\r\n" +
           "    Comment = Commen" +
           "t..CommentLoc(1, sen" +
           "se)\r\n" +
           "  elseif inst.OP == " +
           "26 then\r\n" +
           "    Operand = Operan" +
           "dAC(inst)\r\n" +
           "    local sense = \"" +
           " if false\"\r\n" +
           "    if inst.C == 0 t" +
           "hen sense = \" if tr" +
           "ue\" end\r\n" +
           "    Comment = Commen" +
           "t..CommentLoc(1, sen" +
           "se)\r\n" +
           "  elseif inst.OP == " +
           "28 or\r\n" +
           "         inst.OP == " +
           "29 then\r\n" +
           "    Operand = Operan" +
           "dABC(inst)\r\n" +
           "  elseif inst.OP == " +
           "30 then\r\n" +
           "    Operand = Operan" +
           "dAB(inst)\r\n" +
           "  elseif inst.OP == " +
           "31 then\r\n" +
           "    Operand = Operan" +
           "dAsBx(inst)\r\n" +
           "    Comment = Commen" +
           "tLoc(inst.sBx, \" if" +
           " loop\")\r\n" +
           "  elseif inst.OP == " +
           "32 then\r\n" +
           "    Operand = Operan" +
           "dAsBx(inst)\r\n" +
           "    Comment = Commen" +
           "tLoc(inst.sBx)\r\n" +
           "  elseif inst.OP == " +
           "33 then\r\n" +
           "    Operand = Operan" +
           "dAC(inst)\r\n" +
           "    Comment = Commen" +
           "tLoc(1, \" if exit\"" +
           ")\r\n" +
           "  elseif inst.OP == " +
           "34 then\r\n" +
           "    Operand = Operan" +
           "dABC(inst)\r\n" +
           "    local n = inst.B" +
           "\r\n" +
           "    local c = inst.C" +
           "\r\n" +
           "    if c == 0 then\r" +
           "\n" +
           "      c = func.code[" +
           "pos + 1]\r\n" +
           "      func.inst[pos " +
           "+ 1].prev = true\r\n" +
           "    end\r\n" +
           "    local start = (c" +
           " - 1) * config.FPF +" +
           " 1\r\n" +
           "    local last = sta" +
           "rt + n - 1\r\n" +
           "    Comment = \"inde" +
           "x \"..start..\" to " +
           "\"\r\n" +
           "    if n ~= 0 then\r" +
           "\n" +
           "      Comment = Comm" +
           "ent..last\r\n" +
           "    else\r\n" +
           "      Comment = Comm" +
           "ent..\"top\"\r\n" +
           "    end\r\n" +
           "  elseif inst.OP == " +
           "35 then\r\n" +
           "    Operand = string" +
           ".format(config.FORMA" +
           "T_A, inst.A)\r\n" +
           "  elseif inst.OP == " +
           "36 then\r\n" +
           "    Operand = Operan" +
           "dABx(inst)\r\n" +
           "    Comment = func.p" +
           "[inst.Bx + 1].nups.." +
           "\" upvalues\"\r\n" +
           "  elseif inst.OP == " +
           "37 then\r\n" +
           "    Operand = Operan" +
           "dAB(inst)\r\n" +
           "  else\r\n" +
           "    Operand = string" +
           ".format(\"OP %d\", i" +
           "nst.OP)\r\n" +
           "  end\r\n" +
           "  if Comment and Com" +
           "ment ~= \"\" then\r" +
           "\n" +
           "    Operand = Operan" +
           "d..config.DISPLAY_SE" +
           "P\r\n" +
           "              ..conf" +
           "ig.DISPLAY_COMMENT.." +
           "Comment\r\n" +
           "  end\r\n" +
           "  return LeftJustify" +
           "(inst.opname, config" +
           ".WIDTH_OPCODE)\r\n" +
           "         ..config.DI" +
           "SPLAY_SEP..Operand\r" +
           "\n" +
           "end\r\n" +
           "function SourceInit(" +
           "source)\r\n" +
           "  if config.source t" +
           "hen config.srcprev =" +
           " 0; return end\r\n" +
           "  if not source or s" +
           "ource == \"\" or\r\n" +
           "     string.sub(sour" +
           "ce, 1, 1) ~= \"@\" t" +
           "hen\r\n" +
           "    return\r\n" +
           "  end\r\n" +
           "  source = string.su" +
           "b(source, 2)\r\n" +
           "  for _, fname in ip" +
           "airs(other_files) do" +
           "\r\n" +
           "    if not config.so" +
           "urce then\r\n" +
           "      if fname == so" +
           "urce or\r\n" +
           "         string.lowe" +
           "r(fname) == string.l" +
           "ower(source) then\r" +
           "\n" +
           "        config.sourc" +
           "e = fname\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  if not config.sour" +
           "ce then return end\r" +
           "\n" +
           "  local INF = io.ope" +
           "n(config.source, \"r" +
           "b\")\r\n" +
           "  if not INF then\r" +
           "\n" +
           "    error(\"cannot r" +
           "ead file \\\"\"..fil" +
           "ename..\"\\\"\")\r\n" +
           "  end\r\n" +
           "  config.srcline = {" +
           "}; config.srcmark = " +
           "{}\r\n" +
           "  local n, line = 1" +
           "\r\n" +
           "  repeat\r\n" +
           "    line = INF:read(" +
           "\"*l\")\r\n" +
           "    if line then\r\n" +
           "      config.srcline" +
           "[n], config.srcmark[" +
           "n] = line, false\r\n" +
           "      n = n + 1\r\n" +
           "    end\r\n" +
           "  until not line\r\n" +
           "  io.close(INF)\r\n" +
           "  config.srcsize = n" +
           " - 1\r\n" +
           "  config.DISPLAY_SRC" +
           "_WIDTH = WidthOf(con" +
           "fig.srcsize)\r\n" +
           "  config.srcprev = 0" +
           "\r\n" +
           "end\r\n" +
           "function SourceMark(" +
           "func)\r\n" +
           "  if not config.sour" +
           "ce then return end\r" +
           "\n" +
           "  if func.sizelinein" +
           "fo == 0 then return " +
           "end\r\n" +
           "  for i = 1, func.si" +
           "zelineinfo do\r\n" +
           "    if i <= config.s" +
           "rcsize then\r\n" +
           "      config.srcmark" +
           "[func.lineinfo[i]] =" +
           " true\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "end\r\n" +
           "function SourceMerge" +
           "(func, pc)\r\n" +
           "  if not config.sour" +
           "ce or not config.DIS" +
           "PLAY_FLAG then retur" +
           "n end\r\n" +
           "  local lnum = func." +
           "lineinfo[pc]\r\n" +
           "  if config.srcprev " +
           "== lnum then return " +
           "end\r\n" +
           "  config.srcprev = l" +
           "num\r\n" +
           "  if config.srcsize " +
           "< lnum then return e" +
           "nd\r\n" +
           "  local lfrom = lnum" +
           "\r\n" +
           "  config.srcmark[lnu" +
           "m] = true\r\n" +
           "  while lfrom > 1 an" +
           "d config.srcmark[lfr" +
           "om - 1] == false do" +
           "\r\n" +
           "    lfrom = lfrom - " +
           "1\r\n" +
           "    config.srcmark[l" +
           "from] = true\r\n" +
           "  end\r\n" +
           "  for i = lfrom, lnu" +
           "m do\r\n" +
           "    WriteLine(config" +
           ".DISPLAY_COMMENT\r\n" +
           "              ..\"(" +
           "\"..ZeroPad(i, confi" +
           "g.DISPLAY_SRC_WIDTH)" +
           "..\")\"\r\n" +
           "              ..conf" +
           "ig.DISPLAY_SEP..conf" +
           "ig.srcline[i])\r\n" +
           "  end\r\n" +
           "end\r\n" +
           "function ChunkSpy(ch" +
           "unk_name, chunk)\r\n" +
           "  local idx = 1\r\n" +
           "  local previdx, len" +
           "\r\n" +
           "  local result = {}" +
           "\r\n" +
           "  local stat = {}\r" +
           "\n" +
           "  result.chunk_name " +
           "= chunk_name or \"\"" +
           "\r\n" +
           "  result.chunk_size " +
           "= string.len(chunk)" +
           "\r\n" +
           "  local function Tes" +
           "tChunk(size, idx, er" +
           "rmsg)\r\n" +
           "    if idx + size - " +
           "1 > result.chunk_siz" +
           "e then\r\n" +
           "      error(string.f" +
           "ormat(\"chunk too sm" +
           "all for %s at offset" +
           " %d\", errmsg, idx -" +
           " 1))\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  local function Loa" +
           "dByte()\r\n" +
           "    previdx = idx\r" +
           "\n" +
           "    idx = idx + 1\r" +
           "\n" +
           "    return string.by" +
           "te(chunk, previdx)\r" +
           "\n" +
           "  end\r\n" +
           "  local function Loa" +
           "dBlock(size)\r\n" +
           "    if not pcall(Tes" +
           "tChunk, size, idx, " +
           "\"LoadBlock\") then " +
           "return end\r\n" +
           "    previdx = idx\r" +
           "\n" +
           "    idx = idx + size" +
           "\r\n" +
           "    local b = string" +
           ".sub(chunk, idx - si" +
           "ze, idx - 1)\r\n" +
           "    if config.endian" +
           "ness == 1 then\r\n" +
           "      return b\r\n" +
           "    else\r\n" +
           "      return string." +
           "reverse(b)\r\n" +
           "    end\r\n" +
           "  end\r\n\r\n" +
           "  function FormatLin" +
           "e(size, desc, index," +
           " segment)\r\n" +
           "    if not config.DI" +
           "SPLAY_FLAG or config" +
           ".DISPLAY_BRIEF then " +
           "return end\r\n" +
           "    if config.DISPLA" +
           "Y_HEX_DATA then\r\n" +
           "      if size == 0 t" +
           "hen\r\n" +
           "        WriteLine(Fo" +
           "rmatPos(index)..conf" +
           "ig.DISPLAY_SEP\r\n" +
           "                  .." +
           "config.BLANKS_HEX_DA" +
           "TA..config.DISPLAY_S" +
           "EP\r\n" +
           "                  .." +
           "desc)\r\n" +
           "      else\r\n" +
           "        while size >" +
           " 0 do\r\n" +
           "          local d, d" +
           "len = \"\", size\r\n" +
           "          if size > " +
           "config.WIDTH_HEX the" +
           "n dlen = config.WIDT" +
           "H_HEX end\r\n" +
           "          for i = 0," +
           " dlen - 1 do\r\n" +
           "            d = d..s" +
           "tring.format(\"%02X" +
           "\", string.byte(chun" +
           "k, index + i))\r\n" +
           "          end\r\n" +
           "          d = d..str" +
           "ing.rep(\"  \", conf" +
           "ig.WIDTH_HEX - dlen)" +
           "\r\n" +
           "          if segment" +
           " or size > config.WI" +
           "DTH_HEX then\r\n" +
           "            d = d.." +
           "\"+\"; size = size -" +
           " config.WIDTH_HEX\r" +
           "\n" +
           "          else\r\n" +
           "            d = d.." +
           "\" \"; size = 0\r\n" +
           "          end\r\n" +
           "          if desc th" +
           "en\r\n" +
           "            WriteLin" +
           "e(FormatPos(index).." +
           "config.DISPLAY_SEP\r" +
           "\n" +
           "                    " +
           "  ..d..config.DISPLA" +
           "Y_SEP\r\n" +
           "                    " +
           "  ..desc)\r\n" +
           "            desc = n" +
           "il\r\n" +
           "          else\r\n" +
           "            WriteLin" +
           "e(FormatPos(index).." +
           "config.DISPLAY_SEP.." +
           "d)\r\n" +
           "          end\r\n" +
           "          index = in" +
           "dex + dlen\r\n" +
           "        end\r\n" +
           "      end\r\n" +
           "    else\r\n" +
           "      WriteLine(Form" +
           "atPos(index)..config" +
           ".DISPLAY_SEP..desc)" +
           "\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  DisplayInit(result" +
           ".chunk_size)\r\n" +
           "  HeaderLine() \r\n" +
           "  if result.chunk_na" +
           "me then\r\n" +
           "    FormatLine(0, \"" +
           "** source chunk: \"." +
           ".result.chunk_name, " +
           "idx)\r\n" +
           "    if config.DISPLA" +
           "Y_BRIEF then WriteLi" +
           "ne(config.DISPLAY_CO" +
           "MMENT..\"source chun" +
           "k: \"..result.chunk_" +
           "name) end\r\n" +
           "  end\r\n" +
           "  DescLine(\"** glob" +
           "al header start **\"" +
           ")\r\n" +
           "  len = string.len(c" +
           "onfig.SIGNATURE)\r\n" +
           "  TestChunk(len, idx" +
           ", \"header signature" +
           "\")\r\n" +
           "  if string.sub(chun" +
           "k, 1, len) ~= config" +
           ".SIGNATURE then\r\n" +
           "    error(\"header s" +
           "ignature not found, " +
           "this is not a Lua ch" +
           "unk\")\r\n" +
           "  end\r\n" +
           "  FormatLine(len, \"" +
           "header signature: \"" +
           "..EscapeString(confi" +
           "g.SIGNATURE, 1), idx" +
           ")\r\n" +
           "  idx = idx + len\r" +
           "\n" +
           "  TestChunk(1, idx, " +
           "\"version byte\")\r" +
           "\n" +
           "  result.version = L" +
           "oadByte()\r\n" +
           "  if result.version " +
           "~= config.VERSION th" +
           "en\r\n" +
           "    error(string.for" +
           "mat(\"ChunkSpy canno" +
           "t read version %02X " +
           "chunks\", result.ver" +
           "sion))\r\n" +
           "  end\r\n" +
           "  FormatLine(1, \"ve" +
           "rsion (major:minor h" +
           "ex digits)\", previd" +
           "x)\r\n" +
           "  TestChunk(1, idx, " +
           "\"format byte\")\r\n" +
           "  result.format = Lo" +
           "adByte()\r\n" +
           "  if result.format ~" +
           "= config.FORMAT then" +
           "\r\n" +
           "    error(string.for" +
           "mat(\"ChunkSpy canno" +
           "t read format %02X c" +
           "hunks\", result.form" +
           "at))\r\n" +
           "  end\r\n" +
           "  FormatLine(1, \"fo" +
           "rmat (0=official)\"," +
           " previdx)\r\n" +
           "  TestChunk(1, idx, " +
           "\"endianness byte\")" +
           "\r\n" +
           "  local endianness =" +
           " LoadByte()\r\n" +
           "  if not config.AUTO" +
           "_DETECT then\r\n" +
           "    if endianness ~=" +
           " config.endianness t" +
           "hen\r\n" +
           "      error(\"unsupp" +
           "orted endianness\")" +
           "\r\n" +
           "    end\r\n" +
           "  else\r\n" +
           "    config.endiannes" +
           "s = endianness\r\n" +
           "  end\r\n" +
           "  FormatLine(1, \"en" +
           "dianness (1=little e" +
           "ndian)\", previdx)\r" +
           "\n" +
           "  TestChunk(4, idx, " +
           "\"size bytes\")\r\n" +
           "  local function Tes" +
           "tSize(mysize, sizena" +
           "me, typename)\r\n" +
           "    local byte = Loa" +
           "dByte()\r\n" +
           "    if not config.AU" +
           "TO_DETECT then\r\n" +
           "      if byte ~= con" +
           "fig[mysize] then\r\n" +
           "        error(string" +
           ".format(\"mismatch i" +
           "n %s size (needs %d " +
           "but read %d)\",\r\n" +
           "          sizename, " +
           "config[mysize], byte" +
           "))\r\n" +
           "      end\r\n" +
           "    else\r\n" +
           "      config[mysize]" +
           " = byte\r\n" +
           "    end\r\n" +
           "    FormatLine(1, st" +
           "ring.format(\"size o" +
           "f %s (%s)\", sizenam" +
           "e, typename), previd" +
           "x)\r\n" +
           "  end\r\n" +
           "  TestSize(\"size_in" +
           "t\", \"int\", \"byte" +
           "s\")\r\n" +
           "  TestSize(\"size_si" +
           "ze_t\", \"size_t\", " +
           "\"bytes\")\r\n" +
           "  TestSize(\"size_In" +
           "struction\", \"Instr" +
           "uction\", \"bytes\")" +
           "\r\n" +
           "  TestSize(\"size_lu" +
           "a_Number\", \"number" +
           "\", \"bytes\")\r\n" +
           "  DecodeInit()\r\n" +
           "  TestChunk(1, idx, " +
           "\"integral byte\")\r" +
           "\n" +
           "  config.integral = " +
           "LoadByte()\r\n" +
           "  FormatLine(1, \"in" +
           "tegral (1=integral)" +
           "\", previdx)\r\n" +
           "  local num_id = con" +
           "fig.size_lua_Number " +
           ".. config.integral\r" +
           "\n" +
           "  if not config.AUTO" +
           "_DETECT then\r\n" +
           "    if config.number" +
           "_type ~= LUANUMBER_I" +
           "D[num_id] then\r\n" +
           "      error(\"incorr" +
           "ect lua_Number forma" +
           "t or bad test number" +
           "\")\r\n" +
           "    end\r\n" +
           "  else\r\n" +
           "    config.number_ty" +
           "pe = nil\r\n" +
           "    for i, v in pair" +
           "s(LUANUMBER_ID) do\r" +
           "\n" +
           "      if num_id == i" +
           " then config.number_" +
           "type = v end\r\n" +
           "    end\r\n" +
           "    if not config.nu" +
           "mber_type then\r\n" +
           "      error(\"unreco" +
           "gnized lua_Number ty" +
           "pe\")\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  DescLine(\"* numbe" +
           "r type: \"..config.n" +
           "umber_type)\r\n" +
           "  if config.AUTO_DET" +
           "ECT then\r\n" +
           "    config.descripti" +
           "on = nil\r\n" +
           "    for _, cfg in pa" +
           "irs(CONFIGURATION) d" +
           "o\r\n" +
           "      if cfg.endiann" +
           "ess == config.endian" +
           "ness and\r\n" +
           "         cfg.size_in" +
           "t == config.size_int" +
           " and\r\n" +
           "         cfg.size_si" +
           "ze_t == config.size_" +
           "size_t and\r\n" +
           "         cfg.size_In" +
           "struction == config." +
           "size_Instruction and" +
           "\r\n" +
           "         cfg.size_lu" +
           "a_Number == config.s" +
           "ize_lua_Number and\r" +
           "\n" +
           "         cfg.integra" +
           "l == config.integral" +
           " and\r\n" +
           "         cfg.number_" +
           "type == config.numbe" +
           "r_type then\r\n" +
           "        config.descr" +
           "iption = cfg.descrip" +
           "tion\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    if not config.de" +
           "scription then\r\n" +
           "      config.descrip" +
           "tion = \"chunk platf" +
           "orm unrecognized\"\r" +
           "\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  DescLine(\"* \"..c" +
           "onfig.description)\r" +
           "\n" +
           "  if config.DISPLAY_" +
           "BRIEF then WriteLine" +
           "(config.DISPLAY_COMM" +
           "ENT..config.descript" +
           "ion) end\r\n" +
           "  stat.header = idx " +
           "- 1\r\n" +
           "  DisplayStat(\"* gl" +
           "obal header = \"..st" +
           "at.header..\" bytes" +
           "\")\r\n" +
           "  DescLine(\"** glob" +
           "al header end **\")" +
           "\r\n" +
           "  local function Loa" +
           "dFunction(funcname, " +
           "num, level)\r\n" +
           "    local func = {}" +
           "\r\n" +
           "    local function L" +
           "oadInt()\r\n" +
           "      local x = Load" +
           "Block(config.size_in" +
           "t)\r\n" +
           "      if not x then" +
           "\r\n" +
           "        error(\"coul" +
           "d not load integer\"" +
           ")\r\n" +
           "      else\r\n" +
           "        local sum = " +
           "0\r\n" +
           "        for i = conf" +
           "ig.size_int, 1, -1 d" +
           "o\r\n" +
           "          sum = sum " +
           "* 256 + string.byte(" +
           "x, i)\r\n" +
           "        end\r\n" +
           "        if string.by" +
           "te(x, config.size_in" +
           "t) > 127 then\r\n" +
           "          sum = sum " +
           "- math.ldexp(1, 8 * " +
           "config.size_int)\r\n" +
           "        end\r\n" +
           "        if sum < 0 t" +
           "hen error(\"bad inte" +
           "ger\") end\r\n" +
           "        return sum\r" +
           "\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function L" +
           "oadSize()\r\n" +
           "      local x = Load" +
           "Block(config.size_si" +
           "ze_t)\r\n" +
           "      if not x then" +
           "\r\n" +
           "        return\r\n" +
           "      else\r\n" +
           "        local sum = " +
           "0\r\n" +
           "        for i = conf" +
           "ig.size_size_t, 1, -" +
           "1 do\r\n" +
           "          sum = sum " +
           "* 256 + string.byte(" +
           "x, i)\r\n" +
           "        end\r\n" +
           "        return sum\r" +
           "\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function L" +
           "oadNumber()\r\n" +
           "      local x = Load" +
           "Block(config.size_lu" +
           "a_Number)\r\n" +
           "      if not x then" +
           "\r\n" +
           "        error(\"coul" +
           "d not load lua_Numbe" +
           "r\")\r\n" +
           "      else\r\n" +
           "        local conver" +
           "t_func = convert_fro" +
           "m[config.number_type" +
           "]\r\n" +
           "        if not conve" +
           "rt_func then\r\n" +
           "          error(\"co" +
           "uld not find convers" +
           "ion function for lua" +
           "_Number\")\r\n" +
           "        end\r\n" +
           "        return conve" +
           "rt_func(x)\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function L" +
           "oadString()\r\n" +
           "      local len = Lo" +
           "adSize()\r\n" +
           "      if not len the" +
           "n\r\n" +
           "        error(\"coul" +
           "d not load String\")" +
           "\r\n" +
           "      else\r\n" +
           "        if len == 0 " +
           "then \r\n" +
           "          return nil" +
           "\r\n" +
           "        end\r\n" +
           "        TestChunk(le" +
           "n, idx, \"LoadString" +
           "\")\r\n" +
           "        local s = st" +
           "ring.sub(chunk, idx," +
           " idx + len - 2)\r\n" +
           "        idx = idx + " +
           "len\r\n" +
           "        return s\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function L" +
           "oadLines()\r\n" +
           "      local size = L" +
           "oadInt()\r\n" +
           "      func.pos_linei" +
           "nfo = previdx\r\n" +
           "      func.lineinfo " +
           "= {}\r\n" +
           "      func.sizelinei" +
           "nfo = size\r\n" +
           "      for i = 1, siz" +
           "e do\r\n" +
           "        func.lineinf" +
           "o[i] = LoadInt()\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function L" +
           "oadLocals()\r\n" +
           "      local n = Load" +
           "Int()\r\n" +
           "      func.pos_locva" +
           "rs = previdx\r\n" +
           "      func.locvars =" +
           " {}\r\n" +
           "      func.sizelocva" +
           "rs = n\r\n" +
           "      for i = 1, n d" +
           "o\r\n" +
           "        local locvar" +
           " = {}\r\n" +
           "        locvar.varna" +
           "me = LoadString()\r" +
           "\n" +
           "        locvar.pos_v" +
           "arname = previdx\r\n" +
           "        locvar.start" +
           "pc = LoadInt()\r\n" +
           "        locvar.pos_s" +
           "tartpc = previdx\r\n" +
           "        locvar.endpc" +
           " = LoadInt()\r\n" +
           "        locvar.pos_e" +
           "ndpc = previdx\r\n" +
           "        func.locvars" +
           "[i] = locvar\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function L" +
           "oadUpvalues()\r\n" +
           "      local n = Load" +
           "Int()\r\n" +
           "      if n ~= 0 and " +
           "n~= func.nups then\r" +
           "\n" +
           "        error(string" +
           ".format(\"bad nupval" +
           "ues: read %d, expect" +
           "ed %d\", n, func.nup" +
           "s))\r\n" +
           "        return\r\n" +
           "      end\r\n" +
           "      func.pos_upval" +
           "ues = previdx\r\n" +
           "      func.upvalues " +
           "= {}\r\n" +
           "      func.sizeupval" +
           "ues = n\r\n" +
           "      func.posupvalu" +
           "es = {}\r\n" +
           "      for i = 1, n d" +
           "o\r\n" +
           "        func.upvalue" +
           "s[i] = LoadString()" +
           "\r\n" +
           "        func.posupva" +
           "lues[i] = previdx\r" +
           "\n" +
           "        if not func." +
           "upvalues[i] then\r\n" +
           "          error(\"em" +
           "pty string at index " +
           "\"..(i - 1)..\"in up" +
           "value table\")\r\n" +
           "        end\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function L" +
           "oadConstantKs()\r\n" +
           "      local n = Load" +
           "Int()\r\n" +
           "      func.pos_ks = " +
           "previdx\r\n" +
           "      func.k = {}\r" +
           "\n" +
           "      func.sizek = n" +
           "\r\n" +
           "      func.posk = {}" +
           "\r\n" +
           "      for i = 1, n d" +
           "o\r\n" +
           "        local t = Lo" +
           "adByte()\r\n" +
           "        func.posk[i]" +
           " = previdx\r\n" +
           "        if t == conf" +
           "ig.LUA_TNUMBER then" +
           "\r\n" +
           "          func.k[i] " +
           "= LoadNumber()\r\n" +
           "        elseif t == " +
           "config.LUA_TBOOLEAN " +
           "then\r\n" +
           "          local b = " +
           "LoadByte()\r\n" +
           "          if b == 0 " +
           "then b = false else " +
           "b = true end\r\n" +
           "          func.k[i] " +
           "= b\r\n" +
           "        elseif t == " +
           "config.LUA_TSTRING t" +
           "hen\r\n" +
           "          func.k[i] " +
           "= LoadString()\r\n" +
           "        elseif t == " +
           "config.LUA_TNIL then" +
           "\r\n" +
           "          func.k[i] " +
           "= nil\r\n" +
           "        else\r\n" +
           "          error(\"ba" +
           "d constant type \".." +
           "t..\" at \"..previdx" +
           ")\r\n" +
           "        end\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function L" +
           "oadConstantPs()\r\n" +
           "      local n = Load" +
           "Int()\r\n" +
           "      func.pos_ps = " +
           "previdx\r\n" +
           "      func.p = {}\r" +
           "\n" +
           "      func.sizep = n" +
           "\r\n" +
           "      for i = 1, n d" +
           "o\r\n" +
           "        func.p[i] = " +
           "LoadFunction(func.so" +
           "urce, i - 1, level +" +
           " 1)\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function L" +
           "oadCode()\r\n" +
           "      local size = L" +
           "oadInt()\r\n" +
           "      func.pos_code " +
           "= previdx\r\n" +
           "      func.code = {}" +
           "\r\n" +
           "      func.sizecode " +
           "= size\r\n" +
           "      for i = 1, siz" +
           "e do\r\n" +
           "        func.code[i]" +
           " = LoadBlock(config." +
           "size_Instruction)\r" +
           "\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local start = id" +
           "x\r\n" +
           "    func.stat = {}\r" +
           "\n" +
           "    local function S" +
           "etStat(item)\r\n" +
           "      func.stat[item" +
           "] = idx - start\r\n" +
           "      start = idx\r" +
           "\n" +
           "    end\r\n" +
           "    func.source = Lo" +
           "adString()\r\n" +
           "    func.pos_source " +
           "= previdx\r\n" +
           "    if func.source =" +
           "= \"\" and level == " +
           "1 then func.source =" +
           " funcname end\r\n" +
           "    func.linedefined" +
           " = LoadInt()\r\n" +
           "    func.pos_linedef" +
           "ined = previdx\r\n" +
           "    func.lastlinedef" +
           "ined = LoadInt()\r\n" +
           "    if TestChunk(4, " +
           "idx, \"function head" +
           "er\") then return en" +
           "d\r\n" +
           "    func.nups = Load" +
           "Byte()\r\n" +
           "    func.numparams =" +
           " LoadByte()\r\n" +
           "    func.is_vararg =" +
           " LoadByte()\r\n" +
           "    func.maxstacksiz" +
           "e = LoadByte()\r\n" +
           "    SetStat(\"header" +
           "\")\r\n" +
           "    LoadCode()      " +
           " SetStat(\"code\")\r" +
           "\n" +
           "    LoadConstantKs()" +
           " SetStat(\"consts\")" +
           "\r\n" +
           "    LoadConstantPs()" +
           " SetStat(\"funcs\")" +
           "\r\n" +
           "    LoadLines()     " +
           " SetStat(\"lines\")" +
           "\r\n" +
           "    LoadLocals()    " +
           " SetStat(\"locals\")" +
           "\r\n" +
           "    LoadUpvalues()  " +
           " SetStat(\"upvalues" +
           "\")\r\n" +
           "    return func\r\n" +
           "  end\r\n" +
           "  function DescFunct" +
           "ion(func, num, level" +
           ")\r\n" +
           "    local function B" +
           "riefLine(desc)\r\n" +
           "      if not config." +
           "DISPLAY_FLAG or not " +
           "config.DISPLAY_BRIEF" +
           " then return end\r\n" +
           "      if DISPLAY_IND" +
           "ENT then\r\n" +
           "        WriteLine(st" +
           "ring.rep(config.DISP" +
           "LAY_SEP, level - 1)." +
           ".desc)\r\n" +
           "      else\r\n" +
           "        WriteLine(de" +
           "sc)\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function D" +
           "escString(s, pos)\r" +
           "\n" +
           "      local len = st" +
           "ring.len(s or \"\")" +
           "\r\n" +
           "      if len > 0 the" +
           "n \r\n" +
           "        len = len + " +
           "1\r\n" +
           "        s = s..\"\\0" +
           "\"\r\n" +
           "      end\r\n" +
           "      FormatLine(con" +
           "fig.size_size_t, str" +
           "ing.format(\"string " +
           "size (%s)\", len), p" +
           "os)\r\n" +
           "      if len == 0 th" +
           "en return end\r\n" +
           "      pos = pos + co" +
           "nfig.size_size_t\r\n" +
           "      if len <= conf" +
           "ig.WIDTH_HEX then\r" +
           "\n" +
           "        FormatLine(l" +
           "en, EscapeString(s, " +
           "1), pos)\r\n" +
           "      else\r\n" +
           "        while len > " +
           "0 do\r\n" +
           "          local seg_" +
           "len = config.WIDTH_H" +
           "EX\r\n" +
           "          if len < s" +
           "eg_len then seg_len " +
           "= len end\r\n" +
           "          local seg " +
           "= string.sub(s, 1, s" +
           "eg_len)\r\n" +
           "          s = string" +
           ".sub(s, seg_len + 1)" +
           "\r\n" +
           "          len = len " +
           "- seg_len\r\n" +
           "          FormatLine" +
           "(seg_len, EscapeStri" +
           "ng(seg, 1), pos, len" +
           " > 0)\r\n" +
           "          pos = pos " +
           "+ seg_len\r\n" +
           "        end\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function D" +
           "escLines()\r\n" +
           "      local size = f" +
           "unc.sizelineinfo\r\n" +
           "      local pos = fu" +
           "nc.pos_lineinfo\r\n" +
           "      DescLine(\"* l" +
           "ines:\")\r\n" +
           "      FormatLine(con" +
           "fig.size_int, \"size" +
           "lineinfo (\"..size.." +
           "\")\", pos)\r\n" +
           "      pos = pos + co" +
           "nfig.size_int\r\n" +
           "      local WIDTH = " +
           "WidthOf(size)\r\n" +
           "      DescLine(\"[pc" +
           "] (line)\")\r\n" +
           "      for i = 1, siz" +
           "e do\r\n" +
           "        local s = st" +
           "ring.format(\"[%s] (" +
           "%s)\", ZeroPad(i, WI" +
           "DTH), func.lineinfo[" +
           "i])\r\n" +
           "        FormatLine(c" +
           "onfig.size_int, s, p" +
           "os)\r\n" +
           "        pos = pos + " +
           "config.size_int\r\n" +
           "      end\r\n" +
           "      SourceMark(fun" +
           "c)\r\n" +
           "    end\r\n" +
           "    local function D" +
           "escLocals()\r\n" +
           "      local n = func" +
           ".sizelocvars\r\n" +
           "      DescLine(\"* l" +
           "ocals:\")\r\n" +
           "      FormatLine(con" +
           "fig.size_int, \"size" +
           "locvars (\"..n..\")" +
           "\", func.pos_locvars" +
           ")\r\n" +
           "      for i = 1, n d" +
           "o\r\n" +
           "        local locvar" +
           " = func.locvars[i]\r" +
           "\n" +
           "        DescString(l" +
           "ocvar.varname, locva" +
           "r.pos_varname)\r\n" +
           "        DescLine(\"l" +
           "ocal [\"..(i - 1).." +
           "\"]: \"..EscapeStrin" +
           "g(locvar.varname))\r" +
           "\n" +
           "        BriefLine(\"" +
           ".local\"..config.DIS" +
           "PLAY_SEP..EscapeStri" +
           "ng(locvar.varname, 1" +
           ")\r\n" +
           "                  .." +
           "config.DISPLAY_SEP.." +
           "config.DISPLAY_COMME" +
           "NT..(i - 1))\r\n" +
           "        FormatLine(c" +
           "onfig.size_int, \"  " +
           "startpc (\"..locvar." +
           "startpc..\")\", locv" +
           "ar.pos_startpc)\r\n" +
           "        FormatLine(c" +
           "onfig.size_int, \"  " +
           "endpc   (\"..locvar." +
           "endpc..\")\",locvar." +
           "pos_endpc)\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function D" +
           "escUpvalues()\r\n" +
           "      local n = func" +
           ".sizeupvalues\r\n" +
           "      DescLine(\"* u" +
           "pvalues:\")\r\n" +
           "      FormatLine(con" +
           "fig.size_int, \"size" +
           "upvalues (\"..n..\")" +
           "\", func.pos_upvalue" +
           "s)\r\n" +
           "      for i = 1, n d" +
           "o\r\n" +
           "        local upvalu" +
           "e = func.upvalues[i]" +
           "\r\n" +
           "        DescString(u" +
           "pvalue, func.posupva" +
           "lues[i])\r\n" +
           "        DescLine(\"u" +
           "pvalue [\"..(i - 1)." +
           ".\"]: \"..EscapeStri" +
           "ng(upvalue))\r\n" +
           "        BriefLine(\"" +
           ".upvalue\"..config.D" +
           "ISPLAY_SEP..EscapeSt" +
           "ring(upvalue, 1)\r\n" +
           "                  .." +
           "config.DISPLAY_SEP.." +
           "config.DISPLAY_COMME" +
           "NT..(i - 1))\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function D" +
           "escConstantKs()\r\n" +
           "      local n = func" +
           ".sizek\r\n" +
           "      local pos = fu" +
           "nc.pos_ks\r\n" +
           "      DescLine(\"* c" +
           "onstants:\")\r\n" +
           "      FormatLine(con" +
           "fig.size_int, \"size" +
           "k (\"..n..\")\", pos" +
           ")\r\n" +
           "      for i = 1, n d" +
           "o\r\n" +
           "        local posk =" +
           " func.posk[i]\r\n" +
           "        local CONST " +
           "= \"const [\"..(i - " +
           "1)..\"]: \"\r\n" +
           "        local CONSTB" +
           " = config.DISPLAY_SE" +
           "P..config.DISPLAY_CO" +
           "MMENT..(i - 1)\r\n" +
           "        local k = fu" +
           "nc.k[i]\r\n" +
           "        if type(k) =" +
           "= \"number\" then\r" +
           "\n" +
           "          FormatLine" +
           "(1, \"const type \"." +
           ".config.LUA_TNUMBER," +
           " posk)\r\n" +
           "          FormatLine" +
           "(config.size_lua_Num" +
           "ber, CONST..\"(\"..k" +
           "..\")\", posk + 1)\r" +
           "\n" +
           "          BriefLine(" +
           "\".const\"..config.D" +
           "ISPLAY_SEP..k..CONST" +
           "B)\r\n" +
           "        elseif type(" +
           "k) == \"boolean\" th" +
           "en\r\n" +
           "          FormatLine" +
           "(1, \"const type \"." +
           ".config.LUA_TBOOLEAN" +
           ", posk)\r\n" +
           "          FormatLine" +
           "(1, CONST..\"(\"..to" +
           "string(k)..\")\", po" +
           "sk + 1)\r\n" +
           "          BriefLine(" +
           "\".const\"..config.D" +
           "ISPLAY_SEP..tostring" +
           "(k)..CONSTB)\r\n" +
           "        elseif type(" +
           "k) == \"string\" the" +
           "n\r\n" +
           "          FormatLine" +
           "(1, \"const type \"." +
           ".config.LUA_TSTRING," +
           " posk)\r\n" +
           "          DescString" +
           "(k, posk + 1)\r\n" +
           "          DescLine(C" +
           "ONST..EscapeString(k" +
           ", 1))\r\n" +
           "          BriefLine(" +
           "\".const\"..config.D" +
           "ISPLAY_SEP..EscapeSt" +
           "ring(k, 1)..CONSTB)" +
           "\r\n" +
           "        elseif type(" +
           "k) == \"nil\" then\r" +
           "\n" +
           "          FormatLine" +
           "(1, \"const type \"." +
           ".config.LUA_TNIL, po" +
           "sk)\r\n" +
           "          DescLine(C" +
           "ONST..\"nil\")\r\n" +
           "          BriefLine(" +
           "\".const\"..config.D" +
           "ISPLAY_SEP..\"nil\"." +
           ".CONSTB)\r\n" +
           "        end\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function D" +
           "escConstantPs()\r\n" +
           "      local n = func" +
           ".sizep\r\n" +
           "      DescLine(\"* f" +
           "unctions:\")\r\n" +
           "      FormatLine(con" +
           "fig.size_int, \"size" +
           "p (\"..n..\")\", fun" +
           "c.pos_ps)\r\n" +
           "      for i = 1, n d" +
           "o\r\n" +
           "        DescFunction" +
           "(func.p[i], i - 1, l" +
           "evel + 1)\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function D" +
           "escCode()\r\n" +
           "      local size = f" +
           "unc.sizecode\r\n" +
           "      local pos = fu" +
           "nc.pos_code\r\n" +
           "      DescLine(\"* c" +
           "ode:\")\r\n" +
           "      FormatLine(con" +
           "fig.size_int, \"size" +
           "code (\"..size..\")" +
           "\", pos)\r\n" +
           "      pos = pos + co" +
           "nfig.size_int\r\n" +
           "      func.inst = {}" +
           "\r\n" +
           "      local ISIZE = " +
           "WidthOf(size)\r\n" +
           "      for i = 1, siz" +
           "e do\r\n" +
           "        func.inst[i]" +
           " = {}\r\n" +
           "      end\r\n" +
           "      for i = 1, siz" +
           "e do\r\n" +
           "        DecodeInst(f" +
           "unc.code[i], func.in" +
           "st[i])\r\n" +
           "        local inst =" +
           " func.inst[i]\r\n" +
           "        local d = De" +
           "scribeInst(inst, i, " +
           "func)\r\n" +
           "        d = string.f" +
           "ormat(\"[%s] %s\", Z" +
           "eroPad(i, ISIZE), d)" +
           "\r\n" +
           "        SourceMerge(" +
           "func, i)\r\n" +
           "        FormatLine(c" +
           "onfig.size_Instructi" +
           "on, d, pos)\r\n" +
           "        BriefLine(d)" +
           "\r\n" +
           "        pos = pos + " +
           "config.size_Instruct" +
           "ion\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    DescLine(\"\")\r" +
           "\n" +
           "    BriefLine(\"\")" +
           "\r\n" +
           "    FormatLine(0, \"" +
           "** function [\"..num" +
           "..\"] definition (le" +
           "vel \"..level..\")\"" +
           ",\r\n" +
           "               func." +
           "pos_source)\r\n" +
           "    BriefLine(\"; fu" +
           "nction [\"..num..\"]" +
           " definition (level " +
           "\"..level..\")\")\r" +
           "\n" +
           "    DescLine(\"** st" +
           "art of function **\"" +
           ")\r\n" +
           "    DescString(func." +
           "source, func.pos_sou" +
           "rce)\r\n" +
           "    if func.source =" +
           "= nil then\r\n" +
           "      DescLine(\"sou" +
           "rce name: (none)\")" +
           "\r\n" +
           "    else\r\n" +
           "      DescLine(\"sou" +
           "rce name: \"..Escape" +
           "String(func.source))" +
           "\r\n" +
           "    end\r\n" +
           "    SourceInit(func." +
           "source)\r\n" +
           "    local pos = func" +
           ".pos_linedefined\r\n" +
           "    FormatLine(confi" +
           "g.size_int, \"line d" +
           "efined (\"..func.lin" +
           "edefined..\")\", pos" +
           ")\r\n" +
           "    pos = pos + conf" +
           "ig.size_int\r\n" +
           "    FormatLine(confi" +
           "g.size_int, \"last l" +
           "ine defined (\"..fun" +
           "c.lastlinedefined.." +
           "\")\", pos)\r\n" +
           "    pos = pos + conf" +
           "ig.size_int\r\n" +
           "    FormatLine(1, \"" +
           "nups (\"..func.nups." +
           ".\")\", pos)\r\n" +
           "    FormatLine(1, \"" +
           "numparams (\"..func." +
           "numparams..\")\", po" +
           "s + 1)\r\n" +
           "    FormatLine(1, \"" +
           "is_vararg (\"..func." +
           "is_vararg..\")\", po" +
           "s + 2)\r\n" +
           "    FormatLine(1, \"" +
           "maxstacksize (\"..fu" +
           "nc.maxstacksize..\")" +
           "\", pos + 3)\r\n" +
           "    BriefLine(string" +
           ".format(\"; %d upval" +
           "ues, %d params, %d s" +
           "tacks\",\r\n" +
           "      func.nups, fun" +
           "c.numparams, func.ma" +
           "xstacksize))\r\n" +
           "    BriefLine(string" +
           ".format(\".function%" +
           "s%d %d %d %d\", conf" +
           "ig.DISPLAY_SEP,\r\n" +
           "      func.nups, fun" +
           "c.numparams, func.is" +
           "_vararg, func.maxsta" +
           "cksize))\r\n" +
           "    if config.DISPLA" +
           "Y_FLAG and config.DI" +
           "SPLAY_BRIEF then\r\n" +
           "      DescLines() \r" +
           "\n" +
           "      DescLocals()\r" +
           "\n" +
           "      DescUpvalues()" +
           "\r\n" +
           "      DescConstantKs" +
           "()\r\n" +
           "      DescConstantPs" +
           "()\r\n" +
           "      DescCode()\r\n" +
           "    else\r\n" +
           "      DescCode() \r" +
           "\n" +
           "      DescConstantKs" +
           "()\r\n" +
           "      DescConstantPs" +
           "()\r\n" +
           "      DescLines()\r" +
           "\n" +
           "      DescLocals()\r" +
           "\n" +
           "      DescUpvalues()" +
           "\r\n" +
           "    end\r\n" +
           "    DisplayStat(\"* " +
           "func header   = \".." +
           "func.stat.header..\"" +
           " bytes\")\r\n" +
           "    DisplayStat(\"* " +
           "lines size    = \".." +
           "func.stat.lines..\" " +
           "bytes\")\r\n" +
           "    DisplayStat(\"* " +
           "locals size   = \".." +
           "func.stat.locals..\"" +
           " bytes\")\r\n" +
           "    DisplayStat(\"* " +
           "upvalues size = \".." +
           "func.stat.upvalues.." +
           "\" bytes\")\r\n" +
           "    DisplayStat(\"* " +
           "consts size   = \".." +
           "func.stat.consts..\"" +
           " bytes\")\r\n" +
           "    DisplayStat(\"* " +
           "funcs size    = \".." +
           "func.stat.funcs..\" " +
           "bytes\")\r\n" +
           "    DisplayStat(\"* " +
           "code size     = \".." +
           "func.stat.code..\" b" +
           "ytes\")\r\n" +
           "    func.stat.total " +
           "= func.stat.header +" +
           " func.stat.lines +\r" +
           "\n" +
           "                    " +
           "  func.stat.locals +" +
           " func.stat.upvalues " +
           "+\r\n" +
           "                    " +
           "  func.stat.consts +" +
           " func.stat.funcs +\r" +
           "\n" +
           "                    " +
           "  func.stat.code\r\n" +
           "    DisplayStat(\"* " +
           "TOTAL size    = \".." +
           "func.stat.total..\" " +
           "bytes\")\r\n" +
           "    DescLine(\"** en" +
           "d of function **\\n" +
           "\")\r\n" +
           "    BriefLine(\"; en" +
           "d of function\\n\")" +
           "\r\n" +
           "  end\r\n" +
           "  result.func = Load" +
           "Function(\"(chunk)\"" +
           ", 0, 1)\r\n" +
           "  DescFunction(resul" +
           "t.func, 0, 1)\r\n" +
           "  stat.total = idx -" +
           " 1\r\n" +
           "  DisplayStat(\"* TO" +
           "TAL size = \"..stat." +
           "total..\" bytes\")\r" +
           "\n" +
           "  result.stat = stat" +
           "\r\n" +
           "  FormatLine(0, \"**" +
           " end of chunk **\", " +
           "idx)\r\n" +
           "  return result\r\n" +
           "end\r\n" +
           "function WriteBinary" +
           "Chunk(parsed, tofile" +
           ")\r\n" +
           "  local Buffer = {}" +
           "\r\n" +
           "  if tofile then\r\n" +
           "    if not config.OU" +
           "TPUT_FILE then\r\n" +
           "      error(\"must s" +
           "pecify an output fil" +
           "ename for rewrites\"" +
           ")\r\n" +
           "    else\r\n" +
           "      WriteLine = fu" +
           "nction(s) config.OUT" +
           "PUT_FILE:write(s) en" +
           "d\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  local function Dum" +
           "p(s)\r\n" +
           "    if tofile then W" +
           "riteLine(s) else tab" +
           "le.insert(Buffer, s)" +
           " end\r\n" +
           "  end\r\n" +
           "  local function Dum" +
           "pByte(b)\r\n" +
           "    Dump(string.char" +
           "(b))\r\n" +
           "  end\r\n" +
           "  local function Wri" +
           "teBlock(v)\r\n" +
           "    if config.endian" +
           "ness == 1 then\r\n" +
           "      Dump(v)\r\n" +
           "    else\r\n" +
           "      Dump(string.re" +
           "verse(v))\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  Dump(config.SIGNAT" +
           "URE)\r\n" +
           "  DumpByte(config.VE" +
           "RSION)\r\n" +
           "  DumpByte(config.FO" +
           "RMAT)\r\n" +
           "  DumpByte(config.en" +
           "dianness)\r\n" +
           "  DumpByte(config.si" +
           "ze_int)\r\n" +
           "  DumpByte(config.si" +
           "ze_size_t)\r\n" +
           "  DumpByte(config.si" +
           "ze_Instruction)\r\n" +
           "  DumpByte(config.si" +
           "ze_lua_Number)\r\n" +
           "  DecodeInit()\r\n" +
           "  DumpByte(config.in" +
           "tegral)\r\n\r\n" +
           "  local function Wri" +
           "teFunction(func)\r\n" +
           "    local function W" +
           "riteUnsigned(num, ty" +
           "pe_size)\r\n" +
           "      if not type_si" +
           "ze then type_size = " +
           "config.size_int end" +
           "\r\n" +
           "      local v = \"\"" +
           "\r\n" +
           "      for i = 1, typ" +
           "e_size do\r\n" +
           "        v = v..strin" +
           "g.char(num % 256); n" +
           "um = math.floor(num " +
           "/ 256)\r\n" +
           "      end\r\n" +
           "      WriteBlock(v)" +
           "\r\n" +
           "    end\r\n" +
           "    local function W" +
           "riteNumber(num)\r\n" +
           "      local convert_" +
           "func = convert_to[co" +
           "nfig.number_type]\r" +
           "\n" +
           "      if not convert" +
           "_func then\r\n" +
           "        error(\"coul" +
           "d not find conversio" +
           "n function for lua_N" +
           "umber\")\r\n" +
           "      end\r\n" +
           "      WriteBlock(con" +
           "vert_func(num))\r\n" +
           "    end\r\n" +
           "    local function W" +
           "riteString(str)\r\n" +
           "      if not str the" +
           "n\r\n" +
           "        WriteUnsigne" +
           "d(0, config.size_siz" +
           "e_t)\r\n" +
           "        return\r\n" +
           "      end\r\n" +
           "      str = str..\"" +
           "\\0\"\r\n" +
           "      WriteUnsigned(" +
           "string.len(str), con" +
           "fig.size_size_t)\r\n" +
           "      Dump(str)\r\n" +
           "    end\r\n" +
           "    local function W" +
           "riteLines()\r\n" +
           "      WriteUnsigned(" +
           "func.sizelineinfo)\r" +
           "\n" +
           "      for i = 1, fun" +
           "c.sizelineinfo do Wr" +
           "iteUnsigned(func.lin" +
           "einfo[i]) end\r\n" +
           "    end\r\n" +
           "    local function W" +
           "riteLocals()\r\n" +
           "      WriteUnsigned(" +
           "func.sizelocvars)\r" +
           "\n" +
           "      for i = 1, fun" +
           "c.sizelocvars do\r\n" +
           "        local locvar" +
           " = func.locvars[i]\r" +
           "\n" +
           "        WriteString(" +
           "locvar.varname)\r\n" +
           "        WriteUnsigne" +
           "d(locvar.startpc)\r" +
           "\n" +
           "        WriteUnsigne" +
           "d(locvar.endpc)\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function W" +
           "riteUpvalues()\r\n" +
           "      WriteUnsigned(" +
           "func.sizeupvalues)\r" +
           "\n" +
           "      for i = 1, fun" +
           "c.sizeupvalues do Wr" +
           "iteString(func.upval" +
           "ues[i]) end\r\n" +
           "    end\r\n" +
           "    local function W" +
           "riteConstantKs()\r\n" +
           "      WriteUnsigned(" +
           "func.sizek)\r\n" +
           "      for i = 1, fun" +
           "c.sizek do\r\n" +
           "        local v = fu" +
           "nc.k[i]\r\n" +
           "        if type(v) =" +
           "= \"number\" then\r" +
           "\n" +
           "          DumpByte(c" +
           "onfig.LUA_TNUMBER); " +
           "WriteNumber(v)\r\n" +
           "        elseif type(" +
           "v) == \"boolean\" th" +
           "en\r\n" +
           "          local b = " +
           "0; if v then b = 1 e" +
           "nd\r\n" +
           "          DumpByte(c" +
           "onfig.LUA_TBOOLEAN);" +
           " DumpByte(b)\r\n" +
           "        elseif type(" +
           "v) == \"string\" the" +
           "n\r\n" +
           "          DumpByte(c" +
           "onfig.LUA_TSTRING); " +
           "WriteString(v)\r\n" +
           "        elseif type(" +
           "v) == \"nil\" then\r" +
           "\n" +
           "          DumpByte(c" +
           "onfig.LUA_TNIL)\r\n" +
           "        else\r\n" +
           "          error(\"ba" +
           "d constant type \\\"" +
           "\"..type(v)..\"\\\" " +
           "at \"..i)\r\n" +
           "        end\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "    local function W" +
           "riteConstantPs()\r\n" +
           "      WriteUnsigned(" +
           "func.sizep)\r\n" +
           "      for i = 1, fun" +
           "c.sizep do WriteFunc" +
           "tion(func.p[i]) end" +
           "\r\n" +
           "    end\r\n" +
           "    local function W" +
           "riteCode()\r\n" +
           "      WriteUnsigned(" +
           "func.sizecode)\r\n" +
           "      for i = 1, fun" +
           "c.sizecode do WriteB" +
           "lock(EncodeInst(func" +
           ".inst[i])) end\r\n" +
           "    end\r\n" +
           "    WriteString(func" +
           ".source)\r\n" +
           "    WriteUnsigned(fu" +
           "nc.linedefined)\r\n" +
           "    WriteUnsigned(fu" +
           "nc.lastlinedefined)" +
           "\r\n" +
           "    DumpByte(func.nu" +
           "ps)\r\n" +
           "    DumpByte(func.nu" +
           "mparams)\r\n" +
           "    DumpByte(func.is" +
           "_vararg)\r\n" +
           "    DumpByte(func.ma" +
           "xstacksize)\r\n" +
           "    WriteCode()\r\n" +
           "    WriteConstantKs(" +
           ")\r\n" +
           "    WriteConstantPs(" +
           ")\r\n" +
           "    WriteLines()\r\n" +
           "    WriteLocals()\r" +
           "\n" +
           "    WriteUpvalues()" +
           "\r\n" +
           "  end\r\n" +
           "  WriteFunction(pars" +
           "ed.func)\r\n" +
           "  if not tofile then" +
           " return table.concat" +
           "(Buffer) end\r\n" +
           "end\r\n\r\n" +
           "function ChunkSpy_Do" +
           "Files(files)\r\n" +
           "  local binary_chunk" +
           "s = {}\r\n" +
           "  for i, v in pairs(" +
           "files) do\r\n" +
           "    local filename, " +
           "binchunk\r\n" +
           "    if type(i) == \"" +
           "number\" then\r\n" +
           "      filename = v\r" +
           "\n" +
           "      local INF = io" +
           ".open(filename, \"rb" +
           "\")\r\n" +
           "      if not INF the" +
           "n\r\n" +
           "        error(\"cann" +
           "ot open \\\"\"..file" +
           "name..\"\\\" for rea" +
           "ding\")\r\n" +
           "      end\r\n" +
           "      binchunk = INF" +
           ":read(\"*a\")\r\n" +
           "      io.close(INF)" +
           "\r\n" +
           "    else\r\n" +
           "      filename = i\r" +
           "\n" +
           "      binchunk = v\r" +
           "\n" +
           "    end\r\n" +
           "    if binchunk then" +
           "\r\n" +
           "      local sig = st" +
           "ring.sub(binchunk, 1" +
           ", string.len(config." +
           "SIGNATURE))\r\n" +
           "      if sig == conf" +
           "ig.SIGNATURE then\r" +
           "\n" +
           "        binary_chunk" +
           "s[filename] = binchu" +
           "nk\r\n" +
           "      else\r\n" +
           "        table.insert" +
           "(other_files, filena" +
           "me)\r\n" +
           "      end\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  local done\r\n" +
           "  for i,v in pairs(b" +
           "inary_chunks) do\r\n" +
           "    if done and (con" +
           "fig.REWRITE_FLAG or " +
           "config.RUN_FLAG) the" +
           "n\r\n" +
           "      error(\"can re" +
           "write or run only on" +
           "e file at a time\")" +
           "\r\n" +
           "    end\r\n" +
           "    local result = C" +
           "hunkSpy(i, v); done " +
           "= true\r\n" +
           "    if config.REWRIT" +
           "E_FLAG then\r\n" +
           "      if not SetProf" +
           "ile(config.REWRITE_P" +
           "ROFILE) then\r\n" +
           "        error(\"coul" +
           "d not load profile f" +
           "or writing binary ch" +
           "unk\")\r\n" +
           "      end\r\n" +
           "      if files[i] th" +
           "en\r\n" +
           "        if string.su" +
           "b(result.func.source" +
           ", 1, 1) ~= \"@\" the" +
           "n\r\n" +
           "          result.fun" +
           "c.source = \"@\"..re" +
           "sult.func.source\r\n" +
           "        end\r\n" +
           "      end\r\n" +
           "      WriteBinaryChu" +
           "nk(result, true)\r\n" +
           "    elseif config.RU" +
           "N_FLAG then\r\n" +
           "      if not SetProf" +
           "ile(\"local\") then" +
           "\r\n" +
           "        error(\"coul" +
           "d not load profile f" +
           "or writing binary ch" +
           "unk\")\r\n" +
           "      end\r\n" +
           "      local binchunk" +
           " = WriteBinaryChunk(" +
           "result)\r\n" +
           "      local func, ms" +
           "g = loadstring(binch" +
           "unk, i)\r\n" +
           "      if not func th" +
           "en error(msg) end\r" +
           "\n" +
           "      local sandbox " +
           "= {}\r\n" +
           "      arg_other[0] =" +
           " i \r\n" +
           "      arg = arg_othe" +
           "r\r\n" +
           "      setmetatable(s" +
           "andbox, {__index = _" +
           "G})\r\n" +
           "      setfenv(func, " +
           "sandbox)\r\n" +
           "      func()\r\n" +
           "      return\r\n" +
           "    end\r\n" +
           "  end\r\n" +
           "  if not done then\r" +
           "\n" +
           "    print(title) pri" +
           "nt(\"ChunkSpy: no bi" +
           "nary chunks processe" +
           "d!\")\r\n" +
           "  end\r\n" +
           "end\r\n" +
           "function main()\r\n" +
           "    local i, perform" +
           ", gotfile = 1\r\n" +
           "    local files = {}" +
           "\r\n" +
           "\tconfig.AUTO_DETECT" +
           " = true\r\n" +
           "    config.OUTPUT_FI" +
           "LE = \"" + outfile + "\"\r\n" +
           "    config.DISPLAY_F" +
           "LAG = false\r\n" +
           "    config.REWRITE_F" +
           "LAG = true\r\n" +
           "\tconfig.REWRITE_PRO" +
           "FILE = \"USF4\"\r\n" +
           "    table.insert(fil" +
           "es, \"" + infile + "\");" + " gotfile = tru" +
           "e\r\n" +
           "    OutputInit()\r\n" +
           "    if gotfile then" +
           "\r\n" +
           "      ChunkSpy_DoFil" +
           "es(files)\r\n" +
           "    else\r\n" +
           "      print(title) p" +
           "rint(\"ChunkSpy: not" +
           "hing to do!\")\r\n" +
           "    end\r\n" +
           "    OutputExit()\r\n" +
           "  end\r\n\r\n" +
           "pcall(main)";
    }
}

