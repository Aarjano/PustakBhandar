// Simple script to test API endpoints
// Run with: node test-api.js

const fetch = require('node-fetch');

// Base URL for the API
const API_URL = 'http://localhost:5000/api';

// Test the SimpleAuth healthcheck endpoint
async function testHealthcheck() {
  try {
    console.log('Testing SimpleAuth healthcheck...');
    const response = await fetch(`${API_URL}/simple-auth/healthcheck`);
    
    if (!response.ok) {
      throw new Error(`HTTP error ${response.status}`);
    }
    
    const data = await response.json();
    console.log('Healthcheck successful:', data);
    console.log('Server time:', new Date(data.timestamp).toLocaleString());
    return true;
  } catch (error) {
    console.error('Healthcheck failed:', error.message);
    return false;
  }
}

// Test the SimpleAuth register endpoint
async function testRegister() {
  try {
    console.log('\nTesting SimpleAuth register...');
    
    const userData = {
      firstName: 'Test',
      lastName: 'User',
      email: 'test@example.com',
      password: 'password123',
      membershipId: 'TEST123'
    };
    
    const response = await fetch(`${API_URL}/simple-auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(userData)
    });
    
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: 'Registration failed' }));
      throw new Error(`HTTP error ${response.status}: ${errorData.message}`);
    }
    
    const data = await response.json();
    console.log('Registration successful!');
    console.log('User ID:', data.user.id);
    console.log('Token received:', data.token ? 'Yes' : 'No');
    return true;
  } catch (error) {
    console.error('Registration failed:', error.message);
    return false;
  }
}

// Test the SimpleAuth login endpoint
async function testLogin() {
  try {
    console.log('\nTesting SimpleAuth login...');
    
    const credentials = {
      email: 'test@example.com',
      password: 'password123'
    };
    
    const response = await fetch(`${API_URL}/simple-auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(credentials)
    });
    
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: 'Login failed' }));
      throw new Error(`HTTP error ${response.status}: ${errorData.message}`);
    }
    
    const data = await response.json();
    console.log('Login successful!');
    console.log('User ID:', data.user.id);
    console.log('Token received:', data.token ? 'Yes' : 'No');
    return true;
  } catch (error) {
    console.error('Login failed:', error.message);
    return false;
  }
}

// Test the SimpleAuth admin login endpoint
async function testAdminLogin() {
  try {
    console.log('\nTesting SimpleAuth admin login...');
    
    const credentials = {
      username: 'admin',
      password: 'admin123'
    };
    
    const response = await fetch(`${API_URL}/simple-auth/admin-login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(credentials)
    });
    
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: 'Admin login failed' }));
      throw new Error(`HTTP error ${response.status}: ${errorData.message}`);
    }
    
    const data = await response.json();
    console.log('Admin login successful!');
    console.log('Admin username:', data.admin.username);
    console.log('Token received:', data.token ? 'Yes' : 'No');
    return true;
  } catch (error) {
    console.error('Admin login failed:', error.message);
    return false;
  }
}

// Run the tests
async function runTests() {
  console.log('=====================================');
  console.log('TESTING E-LIBRARY API ENDPOINTS');
  console.log('=====================================');
  
  const healthcheckSuccessful = await testHealthcheck();
  
  if (healthcheckSuccessful) {
    await testRegister();
    await testLogin();
    await testAdminLogin();
  }
  
  console.log('\n=====================================');
  console.log('TESTS COMPLETED');
  console.log('=====================================');
}

runTests(); 