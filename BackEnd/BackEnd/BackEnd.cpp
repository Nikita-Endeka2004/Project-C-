#include <iostream>
#include <vector>
#include <fstream>
#include <Windows.h>
#include <iomanip>
#include <string>

std::string replaceSubstring(std::string& str, const std::string& oldSubstr, const std::string& newSubstr) {
    size_t pos = 0;
    while ((pos = str.find(oldSubstr, pos)) != std::string::npos) {
        str.replace(pos, oldSubstr.length(), newSubstr);
        pos += newSubstr.length();
    }
    return str;
}

double myMath(double x) {
    double result = 0.0;
    if (x > 4 && x < 5) {
        result = cos(x) * (sqrt(5 + pow(x, 4)) + ((pow(log(2 * x), 2)) / (1.2 + pow(x, 3))));
    }
    else if (x > -6 && x < -1) {
        result = pow(x, -22) - 1;
    }
    else {
        result = pow(x, 5) + 5;
    }
    return result;
}

int main()
{
    SetConsoleCP(1251);
    SetConsoleOutputCP(1251);
    double x;
    double result;
    int points = 30;
    double stap = 0.1;
    std::vector<double> arrX(points);
    std::vector<double> arrY(points);

    std::cout << "Введіть х: ";
    std::cin >> x;

    for (int i = 0; i < points; i++) {
        arrY[i] = myMath(x);
        arrX[i] = x;
        x += stap;
    }

    std::ofstream outputFile("dataTest.txt");
    if (!outputFile) {
        std::cout << "Помилка відкриття файлу" << std::endl;
        return 1;
    }

    // Установка локали с разделителем дробной части ","
    outputFile.imbue(std::locale(""));
    std::string temp;
    // Записываем значения x и y в файл с разделителем ","
    for (size_t i = 0; i < arrX.size(); ++i) {
        temp = std::to_string(arrY[i]);
        if (i < arrX.size() - 1 && (arrX[i + 1] - arrX[i] > 1000 || arrY[i + 1] - arrY[i] > 1000)) {
            outputFile << "gap" << std::endl;
        }
        else {
            outputFile << std::fixed << std::setprecision(6) << arrX[i] << ":" << std::right << replaceSubstring(temp, ".", ",") << std::endl;
        }
    }

    // Закрываем файл
    outputFile.close();

    std::cout << "В файл записані дані" << std::endl;

}


