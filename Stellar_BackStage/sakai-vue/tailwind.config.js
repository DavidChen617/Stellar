/** @type {import('tailwindcss').Config} */
module.exports = {
    darkMode: ['selector', '[class*="app-dark"]'],
    content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
    plugins: [require('tailwindcss-primeui')],
    theme: {
        screens: {
            sm: '576px',
            md: '768px',
            lg: '992px',
            xl: '1200px',
            '2xl': '1920px'
        },
        extend: {
            backgroundColor: {
                'custom-blue': '#1da1f2',
                'custom-green': '#00b894',
                'dark-custom-blue': '#212d3f', // 深色模式下的藍色
                
            },
            backgroundImage: {
                'custom-gradient-dark': 'linear-gradient(to bottom, #214d6f, #1a2a3c)'
                
            },
        }
    }
};
