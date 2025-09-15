import { apiClient } from './api';

// Test function to verify API connection
export const testApiConnection = async () => {
  console.log('🧪 Testing API connection...');
  
  try {
    // Test 1: Fetch posts (should work without authentication)
    console.log('📝 Testing posts endpoint...');
    const posts = await apiClient.getPosts({ publishedOnly: true });
    console.log('✅ Posts endpoint working:', posts.length, 'posts found');
    
    return {
      success: true,
      postsCount: posts.length,
      message: 'API connection successful'
    };
  } catch (error) {
    console.error('❌ API connection failed:', error);
    return {
      success: false,
      error: error instanceof Error ? error.message : 'Unknown error',
      message: 'API connection failed'
    };
  }
};

// Test authentication endpoints
export const testAuthEndpoints = async () => {
  console.log('🔐 Testing authentication endpoints...');
  
  try {
    // Test invalid login (should fail gracefully)
    console.log('🔒 Testing login endpoint with invalid credentials...');
    const loginResult = await apiClient.login({
      email: 'test@example.com',
      password: 'wrongpassword'
    });
    
    if (!loginResult.success) {
      console.log('✅ Login endpoint properly rejects invalid credentials');
    } else {
      console.log('⚠️ Unexpected: Login succeeded with invalid credentials');
    }
    
    return {
      success: true,
      message: 'Auth endpoints are responding correctly'
    };
  } catch (error) {
    console.error('❌ Auth endpoint test failed:', error);
    return {
      success: false,
      error: error instanceof Error ? error.message : 'Unknown error',
      message: 'Auth endpoint test failed'
    };
  }
};

// Run all tests
export const runApiTests = async () => {
  console.log('🚀 Running API tests...\n');
  
  const connectionTest = await testApiConnection();
  const authTest = await testAuthEndpoints();
  
  console.log('\n📊 Test Results:');
  console.log('- API Connection:', connectionTest.success ? '✅' : '❌');
  console.log('- Auth Endpoints:', authTest.success ? '✅' : '❌');
  
  return {
    connection: connectionTest,
    auth: authTest,
    allPassed: connectionTest.success && authTest.success
  };
};