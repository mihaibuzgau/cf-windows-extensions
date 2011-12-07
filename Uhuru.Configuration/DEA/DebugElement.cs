﻿// -----------------------------------------------------------------------
// <copyright file="DebugElement.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.DEA
{
    using System.Configuration;
    
    /// <summary>
    /// This configuration class contains debug settings for a DEA runtime.
    /// </summary>
    public class DebugElement : ConfigurationElement
    {
        #region Static Fields

        private static ConfigurationProperty propertyName;
        private static ConfigurationProperty propertyEnvironment;

        private static ConfigurationPropertyCollection properties;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes static members of the DebugElement class.
        /// </summary>
        static DebugElement()
        {
            propertyName = new ConfigurationProperty(
                "name",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            propertyEnvironment = new ConfigurationProperty(
                "environment",
                typeof(EnvironmentCollection),
                new EnvironmentCollection(),
                ConfigurationPropertyOptions.IsRequired);

            properties = new ConfigurationPropertyCollection();

            properties.Add(propertyName);
            properties.Add(propertyEnvironment);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of a debug configuration for a runtime.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, DefaultValue = null)]
        public string Name
        {
            get
            {
                return (string)base[propertyName];
            }

            set
            {
                base[propertyName] = value;
            }
        }

        /// <summary>
        /// Gets a collection of environment variables for a debug configuration.
        /// </summary>
        [ConfigurationProperty("environment", IsRequired = true, DefaultValue = null)]
        public EnvironmentCollection Environment
        {
            get
            {
                return (EnvironmentCollection)base[propertyEnvironment];
            }
        }

        /// <summary>
        /// Override the Properties collection and return our custom one.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }

        #endregion
    }
}
