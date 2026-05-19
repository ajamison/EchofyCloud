/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    './Views/**/*.cshtml',
    './wwwroot/js/**/*.js',
    './wwwroot/assets/js/**/*.js',
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Nunito Sans', 'ui-sans-serif', 'system-ui', 'sans-serif'],
      },
      colors: {
        primary: {
          50:  '#eef2ff',
          100: '#e0e7ff',
          200: '#c7d2fe',
          500: '#6366f1',
          600: '#4f46e5',
          700: '#4338ca',
        },
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
  ],
};
