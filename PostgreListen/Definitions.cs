#pragma warning disable 1591

using System.ComponentModel;
using System.Collections.Generic;

namespace PostgreListen
{
    
    /// <summary>
    /// Query Postgre.
    /// </summary>
    [DisplayName("Listen")]
    public class ListenParameters
    {
        /// <summary>
        /// name of the channel to listen to 
        /// "Listen <channel_name>"
        /// 
        /// </summary>
        [DefaultValue("mynotifications")]
        public string Channel { get; set; }
        
    }
    public class ConnectionInformation
    {
        /// <summary>
        /// Connection string.
        /// </summary>
        [PasswordPropertyText(true)]
        [DefaultValue("\"Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;\"")]
        public string ConnectionString { get; set; }
        /// <summary>
        /// Timeout in seconds.
        /// </summary>
        [DefaultValue(30)]
        public int TimeoutSeconds { get; set; }
    }
    /// <summary>
    /// Return object.
    /// </summary>
    public class Output
    {
        /// <summary>
        /// Request result.
        /// </summary>
        public  BoilerPlate Result { get; set; }
        
    }

   
}