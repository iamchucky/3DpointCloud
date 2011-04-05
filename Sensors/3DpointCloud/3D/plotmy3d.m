close all
clear all
clc

threeD = load('output.txt');

pose.x = threeD(:,1);
pose.y = threeD(:,2);
pose.z = threeD(:,3);
pose.yaw = threeD(:,4);
pose.pitch = threeD(:,5);
pose.roll = threeD(:,6);
K = [-686.869 0 297.687
     0 687.997 218.456
     0 0 1];
px = threeD(:,7);
py = threeD(:,8);
nx = threeD(:,9);
ny = threeD(:,10);
prevP = threeD(:,11:22);
nextP = threeD(:,23:34);
A = threeD(:,35:end-3);

prevPM = [prevP(1,1:4); prevP(1,5:8); prevP(1,9:12)];
nextPM = [nextP(1,1:4); nextP(1,5:8); nextP(1,9:12)];
% AM = [A(1,1:4); A(1,5:8); A(1,9:12); A(1,13:end)];
% [V,D] = eig(AM)
% x0 = V(:,4)
% AM*x0
% x0./x0(4)

x = threeD(:,51);
y = threeD(:,52);
z = threeD(:,53);

B = threeD(:,54);
G = threeD(:,55);
R = threeD(:,56);

a = -pi/2;
b = 0;
c = pi/2;
Trc = [0 0 0.5]';
Rrc = [cos(a)*cos(b), cos(a)*sin(b)*sin(c)-sin(a)*cos(c), cos(a)*sin(b)*cos(c)+sin(a)*sin(c)
    sin(a)*cos(b), sin(a)*sin(b)*sin(c)+cos(a)*cos(c), sin(a)*sin(b)*cos(c)-cos(a)*sin(c)
    -sin(b),	   cos(b)*sin(c),					   cos(b)*cos(c)					 ];
RTrc = [Rrc Trc];

figure(1)
hold on
xlabel('x')
ylabel('y')
zlabel('z')
chair = [   2375.13	-143.208	405.42
            1991.04 -136.353	406.023
            2379.06 219.703     448.206
            2009.45 223.917     451.626
            2357.49 -219.649	784.35  ]./1000;
plot3(chair([1 2],1),chair([1 2],2), chair([1 2],3));
hold on
plot3(chair([1 3],1),chair([1 3],2), chair([1 3],3));
plot3(chair([2 4],1),chair([2 4],2), chair([2 4],3));
plot3(chair([3 4],1),chair([3 4],2), chair([3 4],3));
plot3(chair([1 5],1),chair([1 5],2), chair([1 5],3));
plot3(chair([2 2],1),chair([2 5],2), chair([2 5],3));
plot3(chair([5 2],1),chair([5 5],2), chair([5 5],3));
plot3(chair([1 1],1),chair([1 1],2), [chair(1,3) 0]);
plot3(chair([2 2],1),chair([2 2],2), [chair(2,3) 0]);
plot3(chair([3 3],1),chair([3 3],2), [chair(3,3) 0]);
plot3(chair([4 4],1),chair([4 4],2), [chair(4,3) 0]);
axis equal
view([-pi/4 -pi/4 pi/4]);
for n = 1:size(threeD,1)
if x(n) < 2.4 && x(n) > 1.7 && y(n) < 0.3 && y(n) > -0.3
    pp = plot3(x(n), y(n), z(n),'*', 'LineWidth', 2.0);
    set(pp,'color',[R(n) G(n) B(n)]./255);
    pause(0.0001);
end
% pause(0.0001);
end

figure(2)
hold on
xlabel('x')
ylabel('y')
zlabel('z')
for n = 1:size(threeD,1)
if x(n) < 8 && x(n) > -4 && y(n) < 4 && y(n) > -4
    pp = plot3(x(n), y(n), z(n),'*');
    set(pp,'color',[R(n) G(n) B(n)]./255);
end
% pause(0.0001);
end
axis equal

i = 1;
for n = 1:size(threeD,1)
    if x(n) < 8 && x(n) > -4 && y(n) < 4 && y(n) > -4
        newx(i) = x(n);
        newy(i) = y(n);
        newz(i) = z(n);
        i = i + 1;
    end
end
figure(3)
tri = delaunay(newx,newy);
h = trisurf(tri, newx, newy, newz);
axis vis3d
axis equal
% axis off
% l = light('Position',[-50 -15 29])
% set(gca,'CameraPosition',[208 -50 7687])
% lighting phong
shading interp
colorbar EastOutside
xlabel('x')
ylabel('y')
zlabel('z')
