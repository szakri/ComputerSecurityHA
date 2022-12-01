const PROXY_CONFIG = [
  {
    context: [
      "https://localhost:4200/*",
    ],
    target: "https://localhost:7235",
    secure: true,
    
  }
]

module.exports = PROXY_CONFIG;
