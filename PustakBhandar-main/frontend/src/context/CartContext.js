import React, { createContext, useContext, useState, useEffect } from 'react';
import { userService } from '../services/api';
import { useAuth } from './AuthContext';

// Create the cart context
const CartContext = createContext();

// Custom hook to use the cart context
export const useCart = () => {
  return useContext(CartContext);
};

// Cart provider component
export const CartProvider = ({ children }) => {
  const [cartItems, setCartItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [totalItems, setTotalItems] = useState(0);
  const [totalPrice, setTotalPrice] = useState(0);
  const { isAuthenticated } = useAuth();

  // Load cart from API when user is authenticated
  useEffect(() => {
    if (isAuthenticated()) {
      fetchCart();
    } else {
      // Clear cart when user logs out
      setCartItems([]);
      setTotalItems(0);
      setTotalPrice(0);
    }
  }, [isAuthenticated]);

  // Update totals whenever cart items change
  useEffect(() => {
    calculateTotals();
  }, [cartItems]);

  // Fetch cart from API
  const fetchCart = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await userService.getCart();
      setCartItems(data);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to fetch cart');
      console.error('Error fetching cart:', err);
    } finally {
      setLoading(false);
    }
  };

  // Calculate total items and price
  const calculateTotals = () => {
    const items = cartItems.reduce((total, item) => total + item.quantity, 0);
    const price = cartItems.reduce(
      (total, item) => total + (item.unitPrice * item.quantity), 
      0
    );
    
    setTotalItems(items);
    setTotalPrice(price);
  };

  // Add item to cart
  const addToCart = async (bookId, quantity = 1) => {
    setLoading(true);
    setError(null);
    try {
      if (isAuthenticated()) {
        await userService.addToCart(bookId, quantity);
        await fetchCart(); // Refresh cart after adding item
      } else {
        // Handle guest cart (could implement local storage here)
        console.log('User must be logged in to add items to cart');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to add item to cart');
      console.error('Error adding to cart:', err);
    } finally {
      setLoading(false);
    }
  };

  // Update cart item quantity
  const updateCartItem = async (itemId, quantity) => {
    setLoading(true);
    setError(null);
    try {
      if (isAuthenticated()) {
        await userService.updateCartItem(itemId, quantity);
        await fetchCart(); // Refresh cart after updating
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to update cart item');
      console.error('Error updating cart item:', err);
    } finally {
      setLoading(false);
    }
  };

  // Remove item from cart
  const removeFromCart = async (itemId) => {
    setLoading(true);
    setError(null);
    try {
      if (isAuthenticated()) {
        await userService.removeFromCart(itemId);
        await fetchCart(); // Refresh cart after removing
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to remove item from cart');
      console.error('Error removing from cart:', err);
    } finally {
      setLoading(false);
    }
  };

  // Clear the entire cart
  const clearCart = async () => {
    setLoading(true);
    setError(null);
    try {
      // Assuming the API has a method to clear the cart
      // Otherwise, we'd need to remove each item individually
      setCartItems([]);
      setTotalItems(0);
      setTotalPrice(0);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to clear cart');
      console.error('Error clearing cart:', err);
    } finally {
      setLoading(false);
    }
  };

  // Check if a book is in the cart
  const isInCart = (bookId) => {
    return cartItems.some(item => item.bookId === bookId);
  };

  // Context value
  const value = {
    cartItems,
    loading,
    error,
    totalItems,
    totalPrice,
    addToCart,
    updateCartItem,
    removeFromCart,
    clearCart,
    isInCart,
    refreshCart: fetchCart,
  };

  return (
    <CartContext.Provider value={value}>
      {children}
    </CartContext.Provider>
  );
}; 