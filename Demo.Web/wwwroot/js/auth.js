$(document).ready(function(){
    startAutoRefresh();
});

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
  }
  
  function refreshTokens() {
    const refreshToken = getCookie('refreshToken');
    if (!refreshToken) {
        window.location.href = '/Auth/Index';
        return;
    }
  
    $.ajax({
        url: '/Auth/RefreshToken',
        type: 'POST',
        data: JSON.stringify({ refreshToken: refreshToken }),
        contentType: 'application/json',
        dataType: 'json',
        success: function(response) {
            // Update cookies with new tokens
            console.log(response);
            document.cookie = `token=${response.token};max-age=${60*60*24};path=/;Samesite=Lax`;
            document.cookie = `refreshToken=${response.refreshToken};max-age=${60*60*24};path=/;Samesite=Lax`;
        },
        error: function(){
            window.location.href = '/Auth/Index';
        }
    });
  }
  
  
  $.ajaxSetup({
    beforeSend: function(xhr) {
        const token = getCookie('token');
        if (token) {
            xhr.setRequestHeader('Authorization', 'Bearer ' + token);
        }
    }
  });
  
  $(document).ajaxError(function(event, jqXHR) {
    if (jqXHR.status === 401) {
        refreshTokens();
    }
  });
  
  function startAutoRefresh() {
    const refreshInterval = 25 * 60 * 1000; 
    setInterval(() => {
        const token = getCookie('token');
        if (token) {
            refreshTokens();
        }
    }, refreshInterval);
  }

function logout()
{
  $('#logoutButton').click(function(){
      document.cookie="token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
      document.cookie="refreshToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
      window.location.href="/Auth/Index";
  })
}