using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExpressBookerProject.Utilities
{
    public class AdminSessionManager
    {
        private static AdminSession _currentAdminSession;  // Static variable for the current admin session
        private static readonly object _lockObject = new object(); // Lock object for thread safety

        // Private constructor to prevent instantiation from outside
        private AdminSessionManager() { }

        // Public static method to set the current admin session
        public static bool SetCurrentAdminSession(AdminSession session)
        {
            lock (_lockObject)
            {
                if (_currentAdminSession == null)
                {
                    _currentAdminSession = session;
                    return true;
                }
                return false;
            }
        }

        // Public static method to clear the current admin session
        public static void ClearCurrentAdminSession()
        {
            lock (_lockObject)
            {
                _currentAdminSession = null;
            }
        }

        // Public static method to get the current admin session
        public static AdminSession GetCurrentAdminSession()
        {
            return _currentAdminSession;
        }
    }
}