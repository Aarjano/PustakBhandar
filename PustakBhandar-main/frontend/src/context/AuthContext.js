import React, { createContext, useContext, useState, useEffect } from 'react';
import { authService } from '../services/api';

// Create the auth context
const AuthContext = createContext();

// Custom hook to use the auth context
export const useAuth = () => {
  return useContext(AuthContext);
};

// Auth provider component
export const AuthProvider = ({ children }) => {
  const [currentUser, setCurrentUser] = useState(null);
  const [currentAdmin, setCurrentAdmin] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Load user from localStorage on initial render
  useEffect(() => {
    const user = authService.getCurrentUser();
    const admin = authService.getCurrentAdmin();
    
    setCurrentUser(user);
    setCurrentAdmin(admin);
    setLoading(false);
  }, []);

  // User registration
  const register = async (userData) => {
    setError(null);
    try {
      const response = await authService.register(userData);
      setCurrentUser(response.user);
      return response;
    } catch (err) {
      setError(err.response?.data?.message || 'Registration failed');
      throw err;
    }
  };

  // User login
  const login = async (credentials) => {
    setError(null);
    try {
      const response = await authService.login(credentials);
      setCurrentUser(response.user);
      return response;
    } catch (err) {
      setError(err.response?.data?.message || 'Login failed');
      throw err;
    }
  };

  // Admin login
  const adminLogin = async (credentials) => {
    setError(null);
    try {
      const response = await authService.adminLogin(credentials);
      setCurrentAdmin(response.admin);
      return response;
    } catch (err) {
      setError(err.response?.data?.message || 'Admin login failed');
      throw err;
    }
  };

  // Logout
  const logout = () => {
    authService.logout();
    setCurrentUser(null);
    setCurrentAdmin(null);
  };

  // Check if user is authenticated
  const isAuthenticated = () => {
    return !!currentUser;
  };

  // Check if admin is authenticated
  const isAdminAuthenticated = () => {
    return !!currentAdmin;
  };

  // Check if user is a staff member
  const isStaff = () => {
    return currentUser?.isStaff || false;
  };

  // Context value
  const value = {
    currentUser,
    currentAdmin,
    loading,
    error,
    register,
    login,
    adminLogin,
    logout,
    isAuthenticated,
    isAdminAuthenticated,
    isStaff,
  };

  return (
    <AuthContext.Provider value={value}>
      {!loading && children}
    </AuthContext.Provider>
  );
}; 