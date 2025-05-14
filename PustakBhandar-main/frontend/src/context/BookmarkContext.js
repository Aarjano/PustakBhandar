import React, { createContext, useContext, useState, useEffect } from 'react';
import { userService } from '../services/api';
import { useAuth } from './AuthContext';

// Create the bookmark context
const BookmarkContext = createContext();

// Custom hook to use the bookmark context
export const useBookmarks = () => {
  return useContext(BookmarkContext);
};

// Bookmark provider component
export const BookmarkProvider = ({ children }) => {
  const [bookmarks, setBookmarks] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const { isAuthenticated } = useAuth();

  // Load bookmarks from API when user is authenticated
  useEffect(() => {
    if (isAuthenticated()) {
      fetchBookmarks();
    } else {
      // Clear bookmarks when user logs out
      setBookmarks([]);
    }
  }, [isAuthenticated]);

  // Fetch bookmarks from API
  const fetchBookmarks = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await userService.getBookmarks();
      setBookmarks(data);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to fetch bookmarks');
      console.error('Error fetching bookmarks:', err);
    } finally {
      setLoading(false);
    }
  };

  // Add bookmark
  const addBookmark = async (bookId) => {
    setLoading(true);
    setError(null);
    try {
      if (isAuthenticated()) {
        await userService.addBookmark(bookId);
        await fetchBookmarks(); // Refresh bookmarks after adding
      } else {
        console.log('User must be logged in to bookmark books');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to add bookmark');
      console.error('Error adding bookmark:', err);
    } finally {
      setLoading(false);
    }
  };

  // Remove bookmark
  const removeBookmark = async (bookId) => {
    setLoading(true);
    setError(null);
    try {
      if (isAuthenticated()) {
        await userService.removeBookmark(bookId);
        await fetchBookmarks(); // Refresh bookmarks after removing
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to remove bookmark');
      console.error('Error removing bookmark:', err);
    } finally {
      setLoading(false);
    }
  };

  // Check if a book is bookmarked
  const isBookmarked = (bookId) => {
    return bookmarks.some(bookmark => bookmark.bookId === bookId);
  };

  // Context value
  const value = {
    bookmarks,
    loading,
    error,
    addBookmark,
    removeBookmark,
    isBookmarked,
    refreshBookmarks: fetchBookmarks,
  };

  return (
    <BookmarkContext.Provider value={value}>
      {children}
    </BookmarkContext.Provider>
  );
}; 