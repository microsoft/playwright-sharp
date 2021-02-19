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
using System.Globalization;
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
	/// The Mouse class operates in main-frame CSS pixels relative to the top-left corner
	/// of the viewport.
	/// </para>
	/// <para>Every <c>page</c> object has its own Mouse, accessible with <see cref="IPage.Mouse"/>.</para>
	/// </summary>
	public partial interface IMouse
	{
		/// <summary>
		/// <para>
		/// Shortcut for <see cref="IMouse.MoveAsync"/>, <see cref="IMouse.DownAsync"/>, <see
		/// cref="IMouse.UpAsync"/>.
		/// </para>
		/// </summary>
		/// <param name="x">
		/// </param>
		/// <param name="y">
		/// </param>
		/// <param name="button">Defaults to <c>left</c>.</param>
		/// <param name="clickCount">defaults to 1. See [UIEvent.detail].</param>
		/// <param name="delay">
		/// Time to wait between <c>mousedown</c> and <c>mouseup</c> in milliseconds. Defaults
		/// to 0.
		/// </param>
		Task ClickAsync(decimal x, decimal y, Button button = default, int? clickCount = null, decimal? delay = null);
	
		/// <summary>
		/// <para>
		/// Shortcut for <see cref="IMouse.MoveAsync"/>, <see cref="IMouse.DownAsync"/>, <see
		/// cref="IMouse.UpAsync"/>, <see cref="IMouse.DownAsync"/> and <see cref="IMouse.UpAsync"/>.
		/// </para>
		/// </summary>
		/// <param name="x">
		/// </param>
		/// <param name="y">
		/// </param>
		/// <param name="button">Defaults to <c>left</c>.</param>
		/// <param name="delay">
		/// Time to wait between <c>mousedown</c> and <c>mouseup</c> in milliseconds. Defaults
		/// to 0.
		/// </param>
		Task DblclickAsync(decimal x, decimal y, Button button = default, decimal? delay = null);
	
		/// <summary><para>Dispatches a <c>mousedown</c> event.</para></summary>
		/// <param name="button">Defaults to <c>left</c>.</param>
		/// <param name="clickCount">defaults to 1. See [UIEvent.detail].</param>
		Task DownAsync(Button button = default, int? clickCount = null);
	
		/// <summary><para>Dispatches a <c>mousemove</c> event.</para></summary>
		/// <param name="x">
		/// </param>
		/// <param name="y">
		/// </param>
		/// <param name="steps">defaults to 1. Sends intermediate <c>mousemove</c> events.</param>
		Task MoveAsync(decimal x, decimal y, int? steps = null);
	
		/// <summary><para>Dispatches a <c>mouseup</c> event.</para></summary>
		/// <param name="button">Defaults to <c>left</c>.</param>
		/// <param name="clickCount">defaults to 1. See [UIEvent.detail].</param>
		Task UpAsync(Button button = default, int? clickCount = null);
	}
}