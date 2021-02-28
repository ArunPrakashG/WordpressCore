using System;
using System.Collections.Generic;
using System.Text;

namespace WordpressSharp.Interfaces {
	public interface IRequestBuilder<YRequestType, TReturnType> {
		YRequestType InitializeWithDefaultValues();

		TReturnType Create();
	}
}
