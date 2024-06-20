// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//左導覽列選項控制
document.addEventListener("DOMContentLoaded", function() {
    const navLinks = document.querySelectorAll('.nav-item a');
    navLinks.forEach((link) => {
        const submenu = link.nextElementSibling; // 獲取副選項元素
        if (submenu) {
            submenu.style.display = "none"; // 初始為關閉
            link.addEventListener('click', function(e) {
                e.preventDefault(); // 阻止默認鏈接行為
                e.stopPropagation(); // 阻止事件冒泡
                submenu.style.display = (submenu.style.display === "block") ? "none" : "block";
            });
        }
    });
});


/* global bootstrap: false 不懂這是啥 */
(function () {
    'use strict'
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    tooltipTriggerList.forEach(function (tooltipTriggerEl) {
      new bootstrap.Tooltip(tooltipTriggerEl)
    })
  })()

/*讓時間是變動的*/
setInterval(function () {
    var currentTime = new Date();
    var formattedTime = currentTime.toLocaleTimeString(); // 格式化时间，你也可以使用其他方式格式化
    document.getElementById("timeDisplay").innerText = formattedTime;
}, 1000); // 1000毫秒表示1秒鐘

/*變換導覽列的縮放圖示*/
$(document).ready(function(){
    $("#collapse").on("click", function(){
        $(".d-flex.flex-column.flex-shrink-0.p-3.text-white.bg-dark").toggleClass("active");
        $(".fa-align-left").toggleClass("fa-chevron-circle-right");
    });
});


function reserveClassroom(date, time, classroomId) {
    // 使用相對路徑，這裡可能需要根據你的實際情況進行調整
    window.location.href = '/ClassroomReserve/reserve_classroom?date=' + date + '&time=' + time + '&classroomId=' + classroomId;
}

