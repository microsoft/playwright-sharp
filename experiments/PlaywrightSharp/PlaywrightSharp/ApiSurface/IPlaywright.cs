/*
 * MIT License
 *
 * Copyright (c) Microsoft Corporation.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 *
 * ------------------------------------------------------------------------------ 
 * <auto-generated> 
 * This code was generated by a tool at:
 * /utils/doclint/generateDotnetApi.js
 * 
 * Changes to this file may cause incorrect behavior and will be lost if 
 * the code is regenerated. 
 * </auto-generated> 
 * ------------------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PlaywrightSharp
{
    /// <summary>
	/// <para>
	/// Playwright module provides a method to launch a browser instance. The following
	/// is a typical example of using Playwright to drive automation:
	/// </para>
	/// </summary>
	public partial interface IPlaywright
	{
		/// <summary>
		/// <para>
		/// This object can be used to launch or connect to Chromium, returning instances of
		/// [ChromiumBrowser].
		/// </para>
		/// </summary>
		IBrowserType Chromium { get; set; }
	
		/// <summary>
		/// <para>
		/// This object can be used to launch or connect to Firefox, returning instances of
		/// [FirefoxBrowser].
		/// </para>
		/// </summary>
		IBrowserType Firefox { get; set; }
	
		/// <summary>
		/// <para>
		/// Selectors can be used to install custom selector engines. See <a href="./selectors.md)">Working
		/// with selectors</a> for more information.
		/// </para>
		/// </summary>
		ISelectors Selectors { get; set; }
	
		/// <summary><para>This object can be used to launch or connect to WebKit, returning instances of [WebKitBrowser].</para></summary>
		IBrowserType Webkit { get; set; }
	}
}