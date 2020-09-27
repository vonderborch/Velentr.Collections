// ***********************************************************************
// Assembly         : LockFree.Net
// Component        : NodeCreationSettings.cs
// Author           : Christian Webber
// Created          : 2020-09-27
//
// Version          : 1.0.0
// Last Modified By : Christian Webber
// Last Modified On : 2020-09-27
// ***********************************************************************
// <copyright file="NodeCreationSettings.cs">
//     Copyright © 2020
// </copyright>
// <summary>
//     Settings for node creation
// </summary>
//
// Changelog:
//            - 1.0.0 (2020-09-27) - Initial commit.
// ***********************************************************************

namespace LockFree.Net.Objects
{
    public enum NodeCreationSettings
    {
        PointToNull = 0,

        PointNewNodeToOldNode = 1,

        PointOldNodeToNewNode = 2,
    }
}
