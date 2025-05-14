import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { 
  ShoppingCartIcon, 
  BookmarkIcon, 
  UserIcon,
  MagnifyingGlassIcon,
  ArrowRightOnRectangleIcon,
  Cog6ToothIcon
} from '@heroicons/react/24/outline';
import { Disclosure, Menu, Transition } from '@headlessui/react';
import { useAuth } from '../../context/AuthContext';
import { useCart } from '../../context/CartContext';

const Header = () => {
  const [searchQuery, setSearchQuery] = useState('');
  const [showAnnouncementBanner, setShowAnnouncementBanner] = useState(false);
  const [announcement, setAnnouncement] = useState(null);
  const navigate = useNavigate();
  const { currentUser, currentAdmin, logout, isAuthenticated, isAdminAuthenticated } = useAuth();
  const { totalItems } = useCart();

  // Mock announcement - in a real application, this would come from an API
  useEffect(() => {
    // Simulating fetching an announcement
    const mockAnnouncement = {
      id: 1,
      title: 'Special Offer',
      message: 'Get 10% off on all books this week!',
      isActive: true,
    };
    
    if (mockAnnouncement && mockAnnouncement.isActive) {
      setAnnouncement(mockAnnouncement);
      setShowAnnouncementBanner(true);
    }
  }, []);

  const handleSearch = (e) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      navigate(`/books/search?q=${encodeURIComponent(searchQuery)}`);
    }
  };

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <header className="bg-white shadow">
      {/* Announcement Banner */}
      {showAnnouncementBanner && announcement && (
        <div className="bg-primary-700 text-white p-2 text-center">
          <p className="text-sm font-medium">
            {announcement.title}: {announcement.message}
            <button 
              className="ml-2 text-xs underline"
              onClick={() => setShowAnnouncementBanner(false)}
            >
              Dismiss
            </button>
          </p>
        </div>
      )}

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16">
          <div className="flex items-center">
            {/* Logo */}
            <Link to="/" className="flex-shrink-0 flex items-center">
              <span className="text-2xl font-bold text-primary-600">PustakBhandar</span>
            </Link>

            {/* Navigation Links */}
            <nav className="ml-6 flex space-x-8">
              <Link to="/" className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-600 hover:text-primary-600">
                Home
              </Link>
              <Link to="/books" className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-600 hover:text-primary-600">
                Browse Books
              </Link>
              <Link to="/authors" className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-600 hover:text-primary-600">
                Authors
              </Link>
              <Link to="/genres" className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-600 hover:text-primary-600">
                Genres
              </Link>
            </nav>
          </div>

          <div className="flex items-center">
            {/* Search Box */}
            <form onSubmit={handleSearch} className="w-full mx-auto lg:mx-0 flex">
              <input
                type="text"
                placeholder="Search books..."
                className="px-3 py-2 border border-gray-300 rounded-l-md focus:outline-none focus:ring-1 focus:ring-primary-500 focus:border-primary-500 w-full"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
              <button
                type="submit"
                className="bg-primary-600 hover:bg-primary-700 text-white font-bold py-2 px-4 rounded-r-md"
              >
                <MagnifyingGlassIcon className="h-5 w-5" />
              </button>
            </form>

            {/* Cart Icon with Item Count */}
            <Link to="/cart" className="ml-4 relative">
              <ShoppingCartIcon className="h-6 w-6 text-gray-600 hover:text-primary-600" />
              {totalItems > 0 && (
                <span className="absolute -top-2 -right-2 bg-red-500 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center">
                  {totalItems}
                </span>
              )}
            </Link>

            {/* Bookmarks Icon */}
            {isAuthenticated() && (
              <Link to="/bookmarks" className="ml-4">
                <BookmarkIcon className="h-6 w-6 text-gray-600 hover:text-primary-600" />
              </Link>
            )}

            {/* User Menu */}
            <Menu as="div" className="ml-4 relative">
              <Menu.Button className="flex items-center">
                <UserIcon className="h-6 w-6 text-gray-600 hover:text-primary-600" />
              </Menu.Button>
              <Transition
                enter="transition ease-out duration-100"
                enterFrom="transform opacity-0 scale-95"
                enterTo="transform opacity-100 scale-100"
                leave="transition ease-in duration-75"
                leaveFrom="transform opacity-100 scale-100"
                leaveTo="transform opacity-0 scale-95"
              >
                <Menu.Items className="absolute right-0 mt-2 w-48 bg-white divide-y divide-gray-100 rounded-md shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none z-10">
                  {isAuthenticated() ? (
                    <>
                      <div className="px-4 py-3">
                        <p className="text-sm">Signed in as</p>
                        <p className="text-sm font-medium text-gray-900 truncate">
                          {currentUser.email}
                        </p>
                      </div>
                      <div className="py-1">
                        <Menu.Item>
                          {({ active }) => (
                            <Link
                              to="/profile"
                              className={`${
                                active ? 'bg-gray-100 text-gray-900' : 'text-gray-700'
                              } block px-4 py-2 text-sm`}
                            >
                              Your Profile
                            </Link>
                          )}
                        </Menu.Item>
                        <Menu.Item>
                          {({ active }) => (
                            <Link
                              to="/orders"
                              className={`${
                                active ? 'bg-gray-100 text-gray-900' : 'text-gray-700'
                              } block px-4 py-2 text-sm`}
                            >
                              Your Orders
                            </Link>
                          )}
                        </Menu.Item>
                        <Menu.Item>
                          {({ active }) => (
                            <button
                              onClick={handleLogout}
                              className={`${
                                active ? 'bg-gray-100 text-gray-900' : 'text-gray-700'
                              } block w-full text-left px-4 py-2 text-sm`}
                            >
                              Sign out
                            </button>
                          )}
                        </Menu.Item>
                      </div>
                    </>
                  ) : isAdminAuthenticated() ? (
                    <>
                      <div className="px-4 py-3">
                        <p className="text-sm">Signed in as Admin</p>
                        <p className="text-sm font-medium text-gray-900 truncate">
                          {currentAdmin.username}
                        </p>
                      </div>
                      <div className="py-1">
                        <Menu.Item>
                          {({ active }) => (
                            <Link
                              to="/admin/dashboard"
                              className={`${
                                active ? 'bg-gray-100 text-gray-900' : 'text-gray-700'
                              } block px-4 py-2 text-sm`}
                            >
                              Dashboard
                            </Link>
                          )}
                        </Menu.Item>
                        <Menu.Item>
                          {({ active }) => (
                            <Link
                              to="/admin/books"
                              className={`${
                                active ? 'bg-gray-100 text-gray-900' : 'text-gray-700'
                              } block px-4 py-2 text-sm`}
                            >
                              Manage Books
                            </Link>
                          )}
                        </Menu.Item>
                        <Menu.Item>
                          {({ active }) => (
                            <Link
                              to="/admin/orders"
                              className={`${
                                active ? 'bg-gray-100 text-gray-900' : 'text-gray-700'
                              } block px-4 py-2 text-sm`}
                            >
                              Manage Orders
                            </Link>
                          )}
                        </Menu.Item>
                        <Menu.Item>
                          {({ active }) => (
                            <button
                              onClick={handleLogout}
                              className={`${
                                active ? 'bg-gray-100 text-gray-900' : 'text-gray-700'
                              } block w-full text-left px-4 py-2 text-sm`}
                            >
                              Sign out
                            </button>
                          )}
                        </Menu.Item>
                      </div>
                    </>
                  ) : (
                    <div className="py-1">
                      <Menu.Item>
                        {({ active }) => (
                          <Link
                            to="/login"
                            className={`${
                              active ? 'bg-gray-100 text-gray-900' : 'text-gray-700'
                            } block px-4 py-2 text-sm`}
                          >
                            Login
                          </Link>
                        )}
                      </Menu.Item>
                      <Menu.Item>
                        {({ active }) => (
                          <Link
                            to="/register"
                            className={`${
                              active ? 'bg-gray-100 text-gray-900' : 'text-gray-700'
                            } block px-4 py-2 text-sm`}
                          >
                            Register
                          </Link>
                        )}
                      </Menu.Item>
                      <Menu.Item>
                        {({ active }) => (
                          <Link
                            to="/admin/login"
                            className={`${
                              active ? 'bg-gray-100 text-gray-900' : 'text-gray-700'
                            } block px-4 py-2 text-sm`}
                          >
                            Admin Login
                          </Link>
                        )}
                      </Menu.Item>
                    </div>
                  )}
                </Menu.Items>
              </Transition>
            </Menu>
          </div>
        </div>
      </div>
    </header>
  );
};

export default Header; 