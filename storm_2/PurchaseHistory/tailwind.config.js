/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,js}",
    ".vue-project//src/**/*.{html,js}",
    "./node_modules/flowbite/**/*.js",
  ],
  theme: {
    extend: {
      scale: {
        '105': '1.05',
      },
      transitionDuration: {
        '200': '200ms',
      },
      borderWidth: {
        'cus-1': '1px',
        'cus-2': '2px',

      }, maxWidth: {
        '960': '960px',
      },
      flex: {
        1: "1",
        2: "2",
        3: "3",
      },
      fontSize: {
        8: '8px',
        10: '10px',
        12: '12px',
        sm: '0.8rem',
        base: '1rem',
        xl: '1.25rem',
        '2xl': '1.563rem',
        '3xl': '1.953rem',
        '4xl': '2.441rem',
        '5xl': '3.052rem',
      },
      padding: {
        0: "0px",
        0.5: "0.125rem", // 2px
        1: "0.25rem", // 4px
        1.5: "0.375rem", // 6px
        2: "0.5rem", // 8px
        2.5: "0.625rem", // 10px
        3: "0.75rem", // 12px
        3.5: "0.875rem", // 14px
        4: "1rem", // 16px
        5: "1.25rem", // 20px
        6: "1.5rem", // 24px
        7: "1.75rem", // 28px
        8: "2rem", // 32px
        9: "2.25rem", // 36px
        10: "2.5rem", // 40px
        11: "2.75rem", // 44px
        12: "3rem", // 48px
        14: "3.5rem", // 56px
        16: "4rem", // 64px
        20: "5rem", // 80px
        24: "6rem", // 96px
        28: "7rem", // 112px
        32: "8rem", // 128px
        36: "9rem", // 144px
        // Add other padding values as needed
      },
    },
  },
  plugins: [
    require("flowbite/plugin"),
    function ({ addComponents }) {
      addComponents({
        ".stellar-btn": {
          width: "22rem",
          height: "37px",
          backgroundColor: "rgb(50, 88, 109)",
          color: "#67c1f5",
          transition: "background-color 0.3s ease",
          "&:hover": {
            backgroundColor: "rgba(103, 193, 245, 0.4)",
            color: "#A7BACC",
          },
        },
        ".order tr:nth-child(odd)": {
          backgroundColor: "#14202B",
          color: "white",
          "&:hover": {
            backgroundColor: "rgb(100, 100, 100,0.6)",
          },
        },
        ".order tr:not(:nth-child(odd))": {
          backgroundColor: "#1B2838",
          color: "white",
          "&:hover": {
            backgroundColor: "rgb(100, 100, 100,0.6)",
          },
        },
        ".bg-custom_blue": {
          backgroundColor: "rgb(61, 168, 226)",
        },
        ".text-custom_blue": {
          color: "#7092A5",
        },
        ".bg-detail_blue": {
          backgroundColor: "rgb(34, 44, 58)",
        },
        ".refund_btn": {
          display: "block",
          maxWidth: "100%",
          marginTop: "10px",
          padding: "10px",
          border: "solid 1px",
          borderColor: "#445468",
          fontSize: "15px",
          color: "#ffffff",
          backgroundColor: "#4d6076",
          backgroundImage:
            "url(https://help.steampowered.com/public/images/arrow_right.png)",
          backgroundRepeat: "no-repeat",
          backgroundPosition: "right 14px center",
          backgroundSize: "auto 16px",
        },
        ".card_list .card:not(even)": {
          backgroundColor: "#14202b",
        },
        ".card_list .card:nth-child(even)": {
          backgroundColor: "#fff0",
        },
        ".card_btn": {
          marginTop: "10px",
          color: "#fff",
          backgroundImage: "linear-gradient(to right, #47bfff 0%, #1a44c2 60%)",
          padding: "5px",
          textAlign: "center",
        },
        '.accordion-content': {
          maxHeight: '0',
          overflow: 'hidden',
          padding: '0',
          transition: 'max-height 0.3s ease',
        },
        '.accordion-content.active': {
          maxHeight: '150px',
          overflow: 'auto',
          scrollbarColor: '#3f7596 #0b1117',
        },
        '.rotate': {
          transition: 'transform 0.3s ease',
        },
        '.rotate-180': {
          transform: 'rotate(180deg)',
        },
        '#side-menu-switch:checked + .side-menu': {
          transform: 'translateX(0)',
        },
      });
    },
    function ({ addUtilities }) {
      addUtilities({
        '.opacity-50': {
          opacity: '0.5',
        },
        '.border-blue-dashed': {
          borderColor: '#0000ff',
          borderWidth: '2px',
          borderStyle: 'dashed',
        },
        '.scale-105': {
          transform: 'scale(1.05)',
        },
        '.dragging': {
          opacity: '0.5',
          transition: 'opacity 0.2s ease',
        },
        '.drag-over': {
          borderWidth: '2px',
          borderColor: 'blue',
          borderStyle: 'dashed',
          transform: 'scale(1.05)',
          transition: 'transform 0.2s ease, border-color 0.2s ease',
        },
      }, ['responsive', 'hover']);
    },
  ],
};
// #1D2837
