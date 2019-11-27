// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;

namespace MS.Internal.Text.TextInterface
{
public sealed class LocalizedErrorMsgs
{
	private string _localizedExceptionMsgEnumeratorNotStarted;
	private string _localizedExceptionMsgEnumeratorReachedEnd;
	private object _staticLockForLocalizedExceptionMsgs = new object();

	public string EnumeratorNotStarted
	{
		get {
			Monitor.Enter(_staticLockForLocalizedExceptionMsgs);
			try
			{
				return _localizedExceptionMsgEnumeratorNotStarted;
			}
			finally
			{
				Monitor.Exit(_staticLockForLocalizedExceptionMsgs);
			}
		}
		set
		{
			Monitor.Enter(_staticLockForLocalizedExceptionMsgs);
			try
			{
				_localizedExceptionMsgEnumeratorNotStarted = value;
			}
			finally
			{
				Monitor.Exit(_staticLockForLocalizedExceptionMsgs);
			}
		}
    }

	public string EnumeratorReachedEnd
	{
		get {
			Monitor.Enter(_staticLockForLocalizedExceptionMsgs);
			try
			{
				return _localizedExceptionMsgEnumeratorReachedEnd;
			}
			finally
			{
				Monitor.Exit(_staticLockForLocalizedExceptionMsgs);
			}
		}
		set
		{
			Monitor.Enter(_staticLockForLocalizedExceptionMsgs);
			try
			{
				_localizedExceptionMsgEnumeratorReachedEnd = value;
			}
			finally
			{
				Monitor.Exit(_staticLockForLocalizedExceptionMsgs);
			}
		}
    }
}
}
