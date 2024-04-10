using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;

namespace SessionLogger.Infrastructure.Mapping
{
	public interface IHaveCustomMappings
	{
		void CreateMappings(AutoMapper.Profile profile);
	}
}
