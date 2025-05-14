import axios from 'axios';

const API_URL = 'http://localhost:5000/api';

// Create an axios instance with default config
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor to include auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Book-related API calls
export const bookService = {
  getBooks: async (page = 1, limit = 10, search = '', sort = '') => {
    try {
      const response = await api.get(`/BooksApi?page=${page}&limit=${limit}&search=${search}&sort=${sort}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching books:', error);
      throw error;
    }
  },
  
  getBookById: async (id) => {
    try {
      const response = await api.get(`/BooksApi/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching book with id ${id}:`, error);
      throw error;
    }
  },
  
  getBooksByGenre: async (genreId) => {
    try {
      const response = await api.get(`/BooksApi/genre/${genreId}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching books by genre ${genreId}:`, error);
      throw error;
    }
  },
  
  getBooksByAuthor: async (authorId) => {
    try {
      const response = await api.get(`/BooksApi/author/${authorId}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching books by author ${authorId}:`, error);
      throw error;
    }
  },
  
  searchBooks: async (term) => {
    try {
      const response = await api.get(`/BooksApi/search/${term}`);
      return response.data;
    } catch (error) {
      console.error(`Error searching books with term "${term}":`, error);
      throw error;
    }
  },
  
  // Admin: Create a new book
  createBook: async (bookData) => {
    try {
      const response = await api.post('/BooksApi', bookData);
      return response.data;
    } catch (error) {
      console.error('Error creating book:', error);
      throw error;
    }
  },
  
  // Admin: Update an existing book
  updateBook: async (id, bookData) => {
    try {
      const response = await api.put(`/BooksApi/${id}`, bookData);
      return response.data;
    } catch (error) {
      console.error(`Error updating book ${id}:`, error);
      throw error;
    }
  },
  
  // Admin: Delete a book
  deleteBook: async (id) => {
    try {
      const response = await api.delete(`/BooksApi/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error deleting book ${id}:`, error);
      throw error;
    }
  },
};

// Author-related API calls
export const authorService = {
  getAuthors: async () => {
    try {
      const response = await api.get('/AuthorsApi');
      return response.data;
    } catch (error) {
      console.error('Error fetching authors:', error);
      throw error;
    }
  },
  
  getAuthorById: async (id) => {
    try {
      const response = await api.get(`/AuthorsApi/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching author with id ${id}:`, error);
      throw error;
    }
  },
  
  searchAuthors: async (term) => {
    try {
      const response = await api.get(`/AuthorsApi/search/${term}`);
      return response.data;
    } catch (error) {
      console.error(`Error searching authors with term "${term}":`, error);
      throw error;
    }
  },
};

// Authentication API calls
export const authService = {
  register: async (userData) => {
    try {
      const response = await api.post('/Member/register', userData);
      if (response.data.token) {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('user', JSON.stringify(response.data.user));
      }
      return response.data;
    } catch (error) {
      console.error('Error registering user:', error);
      throw error;
    }
  },
  
  login: async (credentials) => {
    try {
      const response = await api.post('/Member/login', credentials);
      if (response.data.token) {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('user', JSON.stringify(response.data.user));
      }
      return response.data;
    } catch (error) {
      console.error('Error logging in:', error);
      throw error;
    }
  },
  
  adminLogin: async (credentials) => {
    try {
      const response = await api.post('/Admin/login', credentials);
      if (response.data.token) {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('admin', JSON.stringify(response.data.admin));
      }
      return response.data;
    } catch (error) {
      console.error('Error admin login:', error);
      throw error;
    }
  },
  
  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('admin');
  },
  
  getCurrentUser: () => {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  },
  
  getCurrentAdmin: () => {
    const admin = localStorage.getItem('admin');
    return admin ? JSON.parse(admin) : null;
  },
};

// Cart and Bookmarks API calls
export const userService = {
  getBookmarks: async () => {
    try {
      const response = await api.get('/Member/bookmarks');
      return response.data;
    } catch (error) {
      console.error('Error fetching bookmarks:', error);
      throw error;
    }
  },
  
  addBookmark: async (bookId) => {
    try {
      const response = await api.post('/Member/bookmarks', { bookId });
      return response.data;
    } catch (error) {
      console.error(`Error adding bookmark for book ${bookId}:`, error);
      throw error;
    }
  },
  
  removeBookmark: async (bookId) => {
    try {
      const response = await api.delete(`/Member/bookmarks/${bookId}`);
      return response.data;
    } catch (error) {
      console.error(`Error removing bookmark for book ${bookId}:`, error);
      throw error;
    }
  },
  
  getCart: async () => {
    try {
      const response = await api.get('/Member/cart');
      return response.data;
    } catch (error) {
      console.error('Error fetching cart:', error);
      throw error;
    }
  },
  
  addToCart: async (bookId, quantity = 1) => {
    try {
      const response = await api.post('/Member/cart', { bookId, quantity });
      return response.data;
    } catch (error) {
      console.error(`Error adding book ${bookId} to cart:`, error);
      throw error;
    }
  },
  
  updateCartItem: async (itemId, quantity) => {
    try {
      const response = await api.put(`/Member/cart/${itemId}`, { quantity });
      return response.data;
    } catch (error) {
      console.error(`Error updating cart item ${itemId}:`, error);
      throw error;
    }
  },
  
  removeFromCart: async (itemId) => {
    try {
      const response = await api.delete(`/Member/cart/${itemId}`);
      return response.data;
    } catch (error) {
      console.error(`Error removing item ${itemId} from cart:`, error);
      throw error;
    }
  },
};

// Order-related API calls
export const orderService = {
  placeOrder: async (orderData) => {
    try {
      const response = await api.post('/Orders', orderData);
      return response.data;
    } catch (error) {
      console.error('Error placing order:', error);
      throw error;
    }
  },
  
  getOrders: async () => {
    try {
      const response = await api.get('/Orders/member');
      return response.data;
    } catch (error) {
      console.error('Error fetching orders:', error);
      throw error;
    }
  },
  
  getOrderById: async (id) => {
    try {
      const response = await api.get(`/Orders/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching order ${id}:`, error);
      throw error;
    }
  },
  
  cancelOrder: async (id) => {
    try {
      const response = await api.post(`/Orders/${id}/cancel`);
      return response.data;
    } catch (error) {
      console.error(`Error cancelling order ${id}:`, error);
      throw error;
    }
  },
  
  // Staff: Process an order using claim code
  processOrder: async (claimCode) => {
    try {
      const response = await api.post('/Orders/process', { claimCode });
      return response.data;
    } catch (error) {
      console.error(`Error processing order with claim code ${claimCode}:`, error);
      throw error;
    }
  },
};

// Review-related API calls
export const reviewService = {
  getBookReviews: async (bookId) => {
    try {
      const response = await api.get(`/Reviews/book/${bookId}`);
      return response.data;
    } catch (error) {
      console.error(`Error fetching reviews for book ${bookId}:`, error);
      throw error;
    }
  },
  
  addReview: async (reviewData) => {
    try {
      const response = await api.post('/Reviews', reviewData);
      return response.data;
    } catch (error) {
      console.error('Error adding review:', error);
      throw error;
    }
  },
  
  updateReview: async (id, reviewData) => {
    try {
      const response = await api.put(`/Reviews/${id}`, reviewData);
      return response.data;
    } catch (error) {
      console.error(`Error updating review ${id}:`, error);
      throw error;
    }
  },
  
  deleteReview: async (id) => {
    try {
      const response = await api.delete(`/Reviews/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error deleting review ${id}:`, error);
      throw error;
    }
  },
};

// Admin announcement API calls
export const announcementService = {
  getAnnouncements: async () => {
    try {
      const response = await api.get('/Announcements');
      return response.data;
    } catch (error) {
      console.error('Error fetching announcements:', error);
      throw error;
    }
  },
  
  createAnnouncement: async (announcementData) => {
    try {
      const response = await api.post('/Announcements', announcementData);
      return response.data;
    } catch (error) {
      console.error('Error creating announcement:', error);
      throw error;
    }
  },
  
  updateAnnouncement: async (id, announcementData) => {
    try {
      const response = await api.put(`/Announcements/${id}`, announcementData);
      return response.data;
    } catch (error) {
      console.error(`Error updating announcement ${id}:`, error);
      throw error;
    }
  },
  
  deleteAnnouncement: async (id) => {
    try {
      const response = await api.delete(`/Announcements/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Error deleting announcement ${id}:`, error);
      throw error;
    }
  },
}; 