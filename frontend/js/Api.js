const API_BASE_URL = 'https://localhost:5001/api'; // ajuste a porta do seu backend

async function apiRequest(endpoint, method = 'GET', body = null) {
    const token = localStorage.getItem('token');
    const headers = {
        'Content-Type': 'application/json'
    };
    if (token) {
        headers.Authorization = `Bearer ${token}`;
    }

    const config = {
        method,
        headers
    };

    if (body !== null) {
        config.body = JSON.stringify(body);
    }

    const response = await fetch(`${API_BASE_URL}${endpoint}`, config);
    if (!response.ok) {
        let message = 'Erro na requisição';
        try {
            const error = await response.json();
            message = error.erro || error.message || message;
        } catch {
        }
        throw new Error(message);
    }
    return await response.json();
}