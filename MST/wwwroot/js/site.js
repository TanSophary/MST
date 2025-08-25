// Navbar background change on scroll
document.addEventListener("scroll", function () {
    const navbar = document.querySelector(".navbar");
    if (!navbar) return; // safety check
    if (window.scrollY > 50) {
        navbar.classList.add("scrolled");
    } else {
        navbar.classList.remove("scrolled");
    }
});
