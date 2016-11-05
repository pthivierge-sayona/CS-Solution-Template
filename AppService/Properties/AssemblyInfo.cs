﻿#region Copyright
//  Copyright 2016 Patrice Thivierge F.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion
using System.Reflection;
using System.Runtime.InteropServices;
using log4net.Config;

[assembly: XmlConfigurator(ConfigFile = "NewAppService.log4Net.cfg.xml", Watch = true)]

//Company shipping the assembly

[assembly: AssemblyCompany("%ASM_COMPANY%")]

//Friendly name for the assembly

[assembly: AssemblyTitle("%ASM_PRODUCT% Service")]

//Short description of the assembly

[assembly: AssemblyDescription("%ASM_PRODUCT% Windows Service")]
[assembly: AssemblyConfiguration("")]

//Product Name

[assembly: AssemblyProduct("%ASM_PRODUCT%")]

//Copyright information

[assembly: AssemblyCopyright("Copyright %ASM_COMPANY% © %YEAR%")]

//Enumeration indicating the target culture for the assembly

[assembly: AssemblyCulture("")]

//

[assembly: ComVisible(false)]



//Version number expressed as a string

[assembly: AssemblyVersion("1.0.*")]
//[assembly: AssemblyFileVersion("1.0.0.0")]