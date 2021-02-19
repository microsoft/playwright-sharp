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
	/// A Browser is created via <see cref="IBrowserType.LaunchAsync"/>. An example of using
	/// a <see cref="IBrowser"/> to create a <see cref="IPage"/>:
	/// </para>
	/// </summary>
	public partial interface IBrowser
	{
		/// <summary>
		/// <para>
		/// Emitted when Browser gets disconnected from the browser application. This might
		/// happen because of one of the following:
		/// </para>
		/// <list type="bullet">
		/// <item><description>Browser application is closed or crashed.</description></item>
		/// <item><description>The <see cref="IBrowser.CloseAsync"/> method was called.</description></item>
		/// </list>
		/// </summary>
		event EventHandler Disconnected;
	
		/// <summary>
		/// <para>
		/// In case this browser is obtained using <see cref="IBrowserType.LaunchAsync"/>, closes
		/// the browser and all of its pages (if any were opened).
		/// </para>
		/// <para>
		/// In case this browser is connected to, clears all created contexts belonging to this
		/// browser and disconnects from the browser server.
		/// </para>
		/// <para>
		/// The <see cref="IBrowser"/> object itself is considered to be disposed and cannot
		/// be used anymore.
		/// </para>
		/// </summary>
		Task CloseAsync();
	
		/// <summary>
		/// <para>
		/// Returns an array of all open browser contexts. In a newly created browser, this
		/// will return zero browser contexts.
		/// </para>
		/// </summary>
		IReadOnlyCollection<IBrowserContext> Contexts { get; }
	
		/// <summary><para>Indicates that the browser is connected.</para></summary>
		bool IsConnected();
	
		/// <summary><para>Creates a new browser context. It won't share cookies/cache with other browser contexts.</para></summary>
		/// <param name="acceptDownloads">
		/// Whether to automatically download all the attachments. Defaults to <c>false</c>
		/// where all the downloads are canceled.
		/// </param>
		/// <param name="bypassCSP">Toggles bypassing page's Content-Security-Policy.</param>
		/// <param name="colorScheme">
		/// Emulates <c>'prefers-colors-scheme'</c> media feature, supported values are <c>'light'</c>,
		/// <c>'dark'</c>, <c>'no-preference'</c>. See <see cref="IPage.EmulateMediaAsync"/>
		/// for more details. Defaults to '<c>light</c>'.
		/// </param>
		/// <param name="deviceScaleFactor">Specify device scale factor (can be thought of as dpr). Defaults to <c>1</c>.</param>
		/// <param name="extraHttpHeaders">
		/// An object containing additional HTTP headers to be sent with every request. All
		/// header values must be strings.
		/// </param>
		/// <param name="geolocation">
		/// </param>
		/// <param name="hasTouch">Specifies if viewport supports touch events. Defaults to false.</param>
		/// <param name="httpCredentials">
		/// Credentials for <a href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Authentication)">HTTP
		/// authentication</a>.
		/// </param>
		/// <param name="ignoreHttpsErrors">Whether to ignore HTTPS errors during navigation. Defaults to <c>false</c>.</param>
		/// <param name="isMobile">
		/// Whether the <c>meta viewport</c> tag is taken into account and touch events are
		/// enabled. Defaults to <c>false</c>. Not supported in Firefox.
		/// </param>
		/// <param name="javaScriptEnabled">Whether or not to enable JavaScript in the context. Defaults to <c>true</c>.</param>
		/// <param name="locale">
		/// Specify user locale, for example using <c>CultureInfo.CurrentUICulture</c>. Locale
		/// will affect <c>navigator.language</c> value, <c>Accept-Language</c> request header
		/// value as well as number and date formatting rules.
		/// </param>
		/// <param name="offline">Whether to emulate network being offline. Defaults to <c>false</c>.</param>
		/// <param name="permissions">
		/// A list of permissions to grant to all pages in this context. See <see cref="IBrowserContext.GrantPermissionsAsync"/>
		/// for more details.
		/// </param>
		/// <param name="proxy">
		/// Network proxy settings to use with this context. Note that browser needs to be launched
		/// with the global proxy for this option to work. If all contexts override the proxy,
		/// global proxy will be never used and can be any string, for example <c>launch({ proxy:
		/// { server: 'per-context' } })</c>.
		/// </param>
		/// <param name="recordHarOmitContent">
		/// Optional setting to control whether to omit request content from the HAR. Defaults
		/// to <c>false</c>.
		/// </param>
		/// <param name="recordHarPath">Path on the filesystem to write the HAR file to.</param>
		/// <param name="recordVideoDirectory">Path to the directory to put videos into.</param>
		/// <param name="recordVideoSize">
		/// Dimensions of the recorded videos. If not specified the size will be equal to <c>viewport</c>
		/// scaled down to fit into 800x800. If <c>viewport</c> is not configured explicitly
		/// the video size defaults to 800x450. Actual picture of each page will be scaled down
		/// if necessary to fit the specified size.
		/// </param>
		/// <param name="storageState">
		/// Populates context with given storage state. This option can be used to initialize
		/// context with logged-in information obtained via <see cref="IBrowserContext.StorageStateAsync"/>.
		/// </param>
		/// <param name="storageStatePath">
		/// Populates context with given storage state. This option can be used to initialize
		/// context with logged-in information obtained via <see cref="IBrowserContext.StorageStateAsync"/>.
		/// Path to the file with saved storage state.
		/// </param>
		/// <param name="timezoneId">Changes the timezone of the context.</param>
		/// <param name="userAgent">Specific user agent to use in this context.</param>
		Task<IBrowserContext> NewContextAsync(bool? acceptDownloads = null, bool? bypassCSP = null, ColorScheme colorScheme = default, decimal? deviceScaleFactor = null, IEnumerable<KeyValuePair<string, string>> extraHttpHeaders = null, BrowserGeolocation geolocation = null, bool? hasTouch = null, BrowserHttpCredentials httpCredentials = null, bool? ignoreHttpsErrors = null, bool? isMobile = null, bool? javaScriptEnabled = null, CultureInfo locale = null, bool? offline = null, IEnumerable<string> permissions = null, BrowserProxy proxy = null, bool? recordHarOmitContent = null, string recordHarPath = null, string recordVideoDirectory = null, BrowserRecordVideoSize recordVideoSize = null, string storageState = null, string storageStatePath = null, TimeZoneInfo timezoneId = null, string userAgent = null);
	
		/// <summary>
		/// <para>
		/// Creates a new page in a new browser context. Closing this page will close the context
		/// as well.
		/// </para>
		/// <para>
		/// This is a convenience API that should only be used for the single-page scenarios
		/// and short snippets. Production code and testing frameworks should explicitly create
		/// <see cref="IBrowser.NewContextAsync"/> followed by the <see cref="IBrowserContext.NewPageAsync"/>
		/// to control their exact life times.
		/// </para>
		/// </summary>
		/// <param name="acceptDownloads">
		/// Whether to automatically download all the attachments. Defaults to <c>false</c>
		/// where all the downloads are canceled.
		/// </param>
		/// <param name="bypassCSP">Toggles bypassing page's Content-Security-Policy.</param>
		/// <param name="colorScheme">
		/// Emulates <c>'prefers-colors-scheme'</c> media feature, supported values are <c>'light'</c>,
		/// <c>'dark'</c>, <c>'no-preference'</c>. See <see cref="IPage.EmulateMediaAsync"/>
		/// for more details. Defaults to '<c>light</c>'.
		/// </param>
		/// <param name="deviceScaleFactor">Specify device scale factor (can be thought of as dpr). Defaults to <c>1</c>.</param>
		/// <param name="extraHttpHeaders">
		/// An object containing additional HTTP headers to be sent with every request. All
		/// header values must be strings.
		/// </param>
		/// <param name="geolocation">
		/// </param>
		/// <param name="hasTouch">Specifies if viewport supports touch events. Defaults to false.</param>
		/// <param name="httpCredentials">
		/// Credentials for <a href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Authentication)">HTTP
		/// authentication</a>.
		/// </param>
		/// <param name="ignoreHttpsErrors">Whether to ignore HTTPS errors during navigation. Defaults to <c>false</c>.</param>
		/// <param name="isMobile">
		/// Whether the <c>meta viewport</c> tag is taken into account and touch events are
		/// enabled. Defaults to <c>false</c>. Not supported in Firefox.
		/// </param>
		/// <param name="javaScriptEnabled">Whether or not to enable JavaScript in the context. Defaults to <c>true</c>.</param>
		/// <param name="locale">
		/// Specify user locale, for example using <c>CultureInfo.CurrentUICulture</c>. Locale
		/// will affect <c>navigator.language</c> value, <c>Accept-Language</c> request header
		/// value as well as number and date formatting rules.
		/// </param>
		/// <param name="offline">Whether to emulate network being offline. Defaults to <c>false</c>.</param>
		/// <param name="permissions">
		/// A list of permissions to grant to all pages in this context. See <see cref="IBrowserContext.GrantPermissionsAsync"/>
		/// for more details.
		/// </param>
		/// <param name="proxy">
		/// Network proxy settings to use with this context. Note that browser needs to be launched
		/// with the global proxy for this option to work. If all contexts override the proxy,
		/// global proxy will be never used and can be any string, for example <c>launch({ proxy:
		/// { server: 'per-context' } })</c>.
		/// </param>
		/// <param name="recordHarOmitContent">
		/// Optional setting to control whether to omit request content from the HAR. Defaults
		/// to <c>false</c>.
		/// </param>
		/// <param name="recordHarPath">Path on the filesystem to write the HAR file to.</param>
		/// <param name="recordVideoDirectory">Path to the directory to put videos into.</param>
		/// <param name="recordVideoSize">
		/// Dimensions of the recorded videos. If not specified the size will be equal to <c>viewport</c>
		/// scaled down to fit into 800x800. If <c>viewport</c> is not configured explicitly
		/// the video size defaults to 800x450. Actual picture of each page will be scaled down
		/// if necessary to fit the specified size.
		/// </param>
		/// <param name="storageState">
		/// Populates context with given storage state. This option can be used to initialize
		/// context with logged-in information obtained via <see cref="IBrowserContext.StorageStateAsync"/>.
		/// </param>
		/// <param name="storageStatePath">
		/// Populates context with given storage state. This option can be used to initialize
		/// context with logged-in information obtained via <see cref="IBrowserContext.StorageStateAsync"/>.
		/// Path to the file with saved storage state.
		/// </param>
		/// <param name="timezoneId">Changes the timezone of the context.</param>
		/// <param name="userAgent">Specific user agent to use in this context.</param>
		Task<IPage> NewPageAsync(bool? acceptDownloads = null, bool? bypassCSP = null, ColorScheme colorScheme = default, decimal? deviceScaleFactor = null, IEnumerable<KeyValuePair<string, string>> extraHttpHeaders = null, BrowserGeolocation geolocation = null, bool? hasTouch = null, BrowserHttpCredentials httpCredentials = null, bool? ignoreHttpsErrors = null, bool? isMobile = null, bool? javaScriptEnabled = null, CultureInfo locale = null, bool? offline = null, IEnumerable<string> permissions = null, BrowserProxy proxy = null, bool? recordHarOmitContent = null, string recordHarPath = null, string recordVideoDirectory = null, BrowserRecordVideoSize recordVideoSize = null, string storageState = null, string storageStatePath = null, TimeZoneInfo timezoneId = null, string userAgent = null);
	
		/// <summary><para>Returns the browser version.</para></summary>
		string Version { get; }
	}
}