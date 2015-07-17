//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: DataBasePlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary.Config
//	File Name:  DataBasePlus
//	User name:  C1400008
//	Location Time: 2015/7/16 12:00:03
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

namespace TerumoMIS.CoreLibrary.Config
{
    /// <summary>
    ///     数据库相关参数
    /// </summary>
    public abstract class DataBasePlus
    {
        /// <summary>
        ///     默认自增ID列名称
        /// </summary>
        private const string _defaultIdentityName = "ID";

        /// <summary>
        ///     默认自增ID列名称
        /// </summary>
        public string DefaultIdentityName
        {
            get { return _defaultIdentityName; }
        }
    }
}